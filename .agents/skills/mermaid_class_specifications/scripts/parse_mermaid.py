import os
import re
from collections import defaultdict

import sys

root_dir = sys.argv[1] if len(sys.argv) > 1 else r"diagrams\class_mermaid"
dest_file = sys.argv[2] if len(sys.argv) > 2 else r"class_specifications.md"
report_file = sys.argv[3] if len(sys.argv) > 3 else r"extra_public_methods_report.md"

data = defaultdict(lambda: defaultdict(lambda: {'methods': {}, 'properties': set()}))

class_pattern = re.compile(r'class\s+([A-Za-z0-9_]+)(?:\["([^"]+)"\])?\s*\{([\s\S]*?)\}')

def map_layer(class_key, layer_raw, has_methods):
    class_key_lower = class_key.lower().strip()
    if layer_raw:
        layer_raw_lower = layer_raw.lower().strip()
        exact_matches = ["frontend controller", "backend controller", "view model", "dto", "service interface", "repository interface", "service", "repository", "entity", "dbcontext", "hub"]
        if layer_raw_lower in exact_matches:
            return layer_raw_lower
        
        if layer_raw_lower == "interface":
            if class_key_lower.endswith("repository"):
                return "repository interface"
            if class_key_lower.endswith("service") or class_key_lower.endswith("queue"):
                return "service interface"
            return "interface"
        
        if layer_raw_lower == "viewmodel":
            return "view model"
        
        if layer_raw_lower == "signalr hub":
            return "hub"
            
        return layer_raw_lower
    else:
        if not has_methods:
            dto_suffixes = ["dto", "request", "response", "return", "command"]
            vm_suffixes = ["viewmodel", "vm"]
            
            if any(class_key_lower.endswith(s) for s in dto_suffixes):
                return "dto"
            elif any(class_key_lower.endswith(s) for s in vm_suffixes):
                return "view model"
            else:
                return "entity"
        else:
            if "service" in class_key_lower or "queue" in class_key_lower:
                return "service"
            if "repository" in class_key_lower:
                return "repository"
            return "uncategorized"

for dirpath, _, filenames in os.walk(root_dir):
    for filename in filenames:
        if filename.endswith(".mmd"):
            filepath = os.path.join(dirpath, filename)
            with open(filepath, "r", encoding="utf-8") as f:
                content = f.read()
            
            for match in class_pattern.finditer(content):
                node_id = match.group(1).strip()
                label = match.group(2)
                body = match.group(3)
                
                class_key = label.strip() if label else node_id
                
                layer_raw = None
                stereo_match = re.search(r'<<(.+?)>>', body)
                if stereo_match:
                    layer_raw = stereo_match.group(1).strip()
                
                methods = {}
                properties = []
                for line in body.split('\n'):
                    line = line.strip()
                    if not line or line.startswith('%%') or line.startswith('<<') or line.startswith('}'):
                        continue
                        
                    visibility = '+' # Default public
                    if line[0] in ['+', '-', '#', '~']:
                        visibility = line[0]
                        member = line[1:].strip()
                    else:
                        member = line
                        
                    member = re.sub(r'\s*\*\s*$', '', member).strip()
                    
                    if '(' in member and ')' in member:
                        methods[member] = visibility
                    else:
                        properties.append(member)
                
                has_methods = len(methods) > 0
                layer = map_layer(class_key, layer_raw, has_methods)
                
                if layer == "dbcontext" or class_key == "AppDbContext":
                    properties = [p for p in properties if "DbSet" not in p]
                
                for m, vis in methods.items():
                    data[layer][class_key]['methods'][m] = vis
                for p in properties:
                    data[layer][class_key]['properties'].add(p)

def resolve_conflicts():
    if 'view model' in data and 'CartSummaryResponse' in data['view model']:
        if 'dto' not in data:
            data['dto'] = defaultdict(lambda: {'methods': {}, 'properties': set()})
        data['dto']['CartSummaryResponse']['methods'].update(data['view model']['CartSummaryResponse']['methods'])
        data['dto']['CartSummaryResponse']['properties'].update(data['view model']['CartSummaryResponse']['properties'])
        del data['view model']['CartSummaryResponse']

    if 'viewmodel' in data:
        for cls, members in data['viewmodel'].items():
            data['view model'][cls]['methods'].update(members['methods'])
            data['view model'][cls]['properties'].update(members['properties'])
        del data['viewmodel']
        
    if 'signalr hub' in data:
        for cls, members in data['signalr hub'].items():
            data['hub'][cls]['methods'].update(members['methods'])
            data['hub'][cls]['properties'].update(members['properties'])
        del data['signalr hub']

    corrections = {
        'Account': {'PasswordHash': 'string?', 'AccountStatus': 'string?', 'Username': 'string?'},
        'CartItem': {'UserId': 'int?', 'CourseId': 'int?', 'Price': 'decimal?'},
        'ChatParticipant': {'Role': 'string', 'JoinedAt': 'DateTime?'},
        'Coupon': {'IsActive': 'bool?', 'CouponType': 'string?', 'MinOrderValue': 'decimal'},
        'Course': {'Title': 'string'},
        'Gift': {'DeliveryStatus': 'string?'},
        'Instructor': {'ProfessionalTitle': 'string?', 'StripeCountry': 'string?', 'DocumentUrl': 'string?', 'ApprovalStatus': 'string?', 'ExpertiseCategories': 'string?'},
        'InstructorPayout': {'PayoutStatus': 'string'},
        'LearningMaterial': {'MaterialMetadata': 'MaterialMetadata?'},
        'Lesson': {'CourseId': 'int?'},
        'Lockout': {'Reason': None, 'LockoutType': 'string?', 'AccountId': 'int?', 'LockoutEnd': 'DateTime?'},
        'Manager': {'DisplayName': 'string', 'AccountId': None},
        'Message': {'SenderId': 'int?', 'IsSeen': 'bool', 'MessageStatus': 'string', 'Content': 'string'},
        'MessageAttachment': {'MessageId': 'int'},
        'OrderInfo': {'OrderDate': 'DateTime?', 'UserId': 'int?', 'OrderStatus': 'string?'},
        'OrderItem': {'CourseId': 'int?', 'OrderId': 'int?'},
        'QuizQuestion': {'LessonId': 'int'},
        'Transaction': {'AccountTo': 'int?', 'AccountFrom': 'int?'},
        'User': {'FullName': 'string', 'DateOfBirth': 'DateOnly?', 'AccountId': None, 'Account': 'Account'},
        'ChatListDto': {'ChatType': 'string'},
        'PagedResult~TransactionListDto~': {'Items': 'IEnumerable<TransactionListDto>'},
        'CartViewModel': {'Items': 'List<CartItemViewModel>', 'AvailableCoupons': 'List<AvailableCouponViewModel>'},
        'PayoutDetailVM': {'PayoutStatus': 'string'}
    }

    for layer, classes in data.items():
        for cls_name, corrections_dict in corrections.items():
            if cls_name in classes:
                props = list(classes[cls_name]['properties'])
                for p in props:
                    p_name = p.split(':')[0].strip()
                    if p_name in corrections_dict:
                        correct_type = corrections_dict[p_name]
                        classes[cls_name]['properties'].remove(p)
                        if correct_type is not None:
                            classes[cls_name]['properties'].add(f"{p_name} : {correct_type}")

    method_removals = {
        'ICouponService': ['GetAllAsync(userId: int, isActive: bool?, type: string?, search: string?, isAdmin: bool)', 'GetAllAsync(managerId: int?, isActive: bool?, type: string?, search: string?, isAdmin: bool)'],
        'CouponService': ['GetAllAsync(userId: int, isActive: bool?, type: string?, search: string?, isAdmin: bool)', 'GetAllAsync(managerId: int?, isActive: bool?, type: string?, search: string?, isAdmin: bool)'],
        'IInstructorService': ['GetPayoutsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?)'],
        'InstructorService': ['GetPayoutsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?)'],
        'INotificationService': ['SendNotificationAsync(accountId: int, title: string, content: string)', 'SendNotificationAsync(receiverId: int, title: string, content: string, actionUrl: string?)', 'SendNotificationAsync(userId: int, title: string, content: string, link: string)', 'SendNotificationAsync(userId: int, title: string, message: string, link: string?)'],
        'NotificationService': ['SendNotificationAsync(accountId: int, title: string, content: string)', 'SendNotificationAsync(userId: int, title: string, content: string, link: string)'],
        'IOtpService': ['ConsumeOtp(email: string, otp: string, context: string)'],
        'OtpService': ['ConsumeOtp(email: string, otp: string, context: string)'],
        'IPaymentGatewayService': ['CreatePaymentIntentAsync(totalAmount: decimal, currency: string, metadata: Dictionary)'],
        'PaymentGatewayService': ['CreatePaymentIntentAsync(totalAmount: decimal, currency: string, metadata: Dictionary)'],
        'ITransactionService': ['GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task<PagedResultTransactionListDto>'],
        'TransactionService': ['GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task<PagedResultTransactionListDto>'],
        'ICartService': ['GetCartSummaryAsync(userId: int, couponCode: string?) : Task<CartSummaryResponse>'],
        'CartService': ['GetCartSummaryAsync(userId: int, couponCode: string?) : Task<CartSummaryResponse>'],
        'IStripeConnectService': ['GetPlatformBalanceAsync() : Task<StripePlatformBalanceDto>'],
        'StripeConnectService': ['GetPlatformBalanceAsync() : Task<StripePlatformBalanceDto>'],
        'IAdminFinanceService': ['GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task<PagedResult>Transaction<>'],
        'AdminFinanceService': ['GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task<PagedResult>Transaction<>'],
        'ICourseRepository': ['GetByIdAsync(courseId: int)', 'IsEnrolledAsync(userId: int, courseId: int)'],
        'CourseRepository': ['GetByIdAsync(courseId: int)', 'IsEnrolledAsync(userId: int, courseId: int)'],
        'IEnrollmentRepository': ['GetMyEnrolledCoursesAsync(id: int)'],
        'EnrollmentRepository': ['GetMyEnrolledCoursesAsync(id: int)'],
        'IInstructorRepository': ['GetByIdAsync(userId: int)', 'GetByIdWithNavigationAsync(instructorId: int)'],
        'InstructorRepository': ['GetByIdAsync(userId: int)', 'GetByIdWithNavigationAsync(instructorId: int)'],
        'ILockoutRepository': ['GetActiveLockoutAsync(id: int, type: string)'],
        'LockoutRepository': ['GetActiveLockoutAsync(id: int, type: string)'],
        'IUserRepository': ['GetAccountByIdAsync(id: int)'],
        'UserRepository': ['GetAccountByIdAsync(id: int)']
    }
    
    method_additions = {
        'ICouponService': [('GetAll(managerId: int?, isActive: bool?, type: string?, search: string?, isAdmin: bool) : Task<List<CouponResponseDTO>>', '+')],
        'CouponService': [('GetAll(managerId: int?, isActive: bool?, type: string?, search: string?, isAdmin: bool) : Task<List<CouponResponseDTO>>', '+')]
    }

    for layer, classes in data.items():
        for cls_name, rem_list in method_removals.items():
            if cls_name in classes:
                methods = list(classes[cls_name]['methods'].keys())
                for m in methods:
                    for rem_str in rem_list:
                        if m.startswith(rem_str) or m == rem_str:
                            del classes[cls_name]['methods'][m]
                            
        for cls_name, add_list in method_additions.items():
            if cls_name in classes:
                for add_str, vis in add_list:
                    classes[cls_name]['methods'][add_str] = vis

def enforce_interface_implementations():
    # Gather all interfaces
    all_interfaces = {}
    for layer in ["interface", "service interface", "repository interface"]:
        if layer in data:
            for cls_name, cls_data in data[layer].items():
                all_interfaces[cls_name] = cls_data
                
    # Gather all classes that might implement them
    all_classes = {}
    for layer, classes in data.items():
        if "interface" not in layer:
            for cls_name, cls_data in classes.items():
                all_classes[cls_name] = (layer, cls_data)
                
    report = ["# Extra Public Methods Report", "Methods present in the class but not in its interface.", ""]
                
    for iface_name, iface_data in all_interfaces.items():
        if iface_name.startswith('I') and len(iface_name) > 1 and iface_name[1].isupper():
            impl_name = iface_name[1:]
        else:
            continue
            
        if impl_name in all_classes:
            impl_layer, impl_data = all_classes[impl_name]
            
            def normalize_method(m):
                m = m.replace('&lt;', '<').replace('&gt;', '>')
                m = m.replace('~', '<').replace('~', '>')
                if ' : ' in m:
                    m = m.split(' : ')[0].strip()
                m = re.sub(r'\s*:\s*', ':', m)
                return m
            
            iface_methods_norm = {normalize_method(m): m for m in iface_data['methods'].keys()}
            impl_methods_norm = {normalize_method(m): m for m in impl_data['methods'].keys()}
            
            # Add missing methods from interface to class
            missing = set(iface_methods_norm.keys()) - set(impl_methods_norm.keys())
            for m_norm in missing:
                original_iface_m = iface_methods_norm[m_norm]
                vis = iface_data['methods'][original_iface_m]
                impl_data['methods'][original_iface_m] = vis
                
            # Find extra methods in class
            extra = set(impl_methods_norm.keys()) - set(iface_methods_norm.keys())
            extra_public = []
            for m_norm in extra:
                original_impl_m = impl_methods_norm[m_norm]
                vis = impl_data['methods'][original_impl_m]
                if vis == '+' or vis == '':  # Public or implicitly public
                    extra_public.append(original_impl_m)
                    
            if extra_public:
                report.append(f"## {impl_name} (Implements {iface_name})")
                for m in extra_public:
                    report.append(f"- `{m}`")
                report.append("")
                
    with open(report_file, "w", encoding="utf-8") as f:
        f.write("\n".join(report))
        
resolve_conflicts()
enforce_interface_implementations()

md_lines = []
md_lines.append("# Class Specifications")
md_lines.append("")

def split_method(m):
    paren_start = m.find('(')
    paren_end = m.find(')')
    
    name = m[:paren_start].strip()
    args = m[paren_start+1:paren_end].strip()
    ret = m[paren_end+1:].strip()
    if ret.startswith(':'):
        ret = ret[1:].strip()
    return name, args, ret

def extract_prop_name(p):
    if ':' in p:
        return p.split(':')[0].strip()
    else:
        parts = p.split()
        if len(parts) >= 2:
            return parts[-1]
        return p

def fix_html_entities(text):
    text = text.replace('<', '&lt;').replace('>', '&gt;')
    while '~' in text:
        text = text.replace('~', '&lt;', 1)
        text = text.replace('~', '&gt;', 1)
    return text

def generate_property_desc(prop_raw, layer):
    prop_name = extract_prop_name(prop_raw)
    is_dependency = prop_name.startswith('_')
    
    clean_name = prop_name.lstrip('_')
    clean_name = re.sub('([a-z])([A-Z])', r'\1 \2', clean_name).strip()
    
    words = clean_name.split()
    words = [w.capitalize() for w in words]
    clean_name = ' '.join(words)
    
    clean_name = re.sub(r'\bId\b', 'ID', clean_name)
    
    if is_dependency or (prop_name.endswith("Service") or prop_name.endswith("Repository") or prop_name.endswith("Context")):
        return f"Provides the {clean_name} dependency."
    
    if clean_name.startswith("Is ") or clean_name.startswith("Has ") or clean_name.startswith("Can "):
        return f"Indicates whether it {clean_name.lower()}."
        
    return f"Represents the {clean_name}."

for layer, classes in sorted(data.items()):
    layer_title = " ".join(word.capitalize() for word in layer.split())
    md_lines.append(f"## Layer: {layer_title}")
    
    class_idx = 1
    for class_key, members in sorted(classes.items()):
        type_label = "Interface" if "interface" in layer.lower() else "Class"
        md_lines.append(f"### {class_idx}. {class_key} {type_label}")
        md_lines.append("| No | Method / Property | Description |")
        md_lines.append("|---|---|---|")
        
        member_idx = 1
        
        for m in sorted(list(members['methods'].keys())):
            name, args, ret = split_method(m)
            
            is_async = False
            if name.endswith("Async"):
                name = name[:-5]
                is_async = True
            if "Task" in ret:
                is_async = True
                
            name_words = re.sub('([a-z])([A-Z])', r'\1 \2', name).strip()
            
            ret_display = ret if ret else "void"
            if ret_display.replace('~', '<') == "Task<IActionResult>" or ret_display == "IActionResult":
                ret_desc = "an HTTP action response"
            elif ret_display:
                ret_safe = fix_html_entities(ret_display).replace('&lt;', '<').replace('&gt;', '>')
                ret_desc = f"`{ret_safe}`"
            else:
                ret_desc = "void"
            
            vis = members['methods'][m]
            
            if "interface" in layer.lower():
                desc = f'Defines the contract to execute the "{name_words}" operation'
            else:
                if vis not in ['-', '#', '~'] and layer.lower() in ['service', 'repository']:
                    desc = f'Provides the concrete implementation to execute the "{name_words}" operation'
                else:
                    desc = f'Executes the "{name_words}" operation'
                
            if is_async:
                desc += " asynchronously"
            desc += f", returning {ret_desc}."
            
            if args:
                args_safe = fix_html_entities(args).replace('&lt;', '<').replace('&gt;', '>')
                desc += f" It accepts `{args_safe}` as input parameters."
                
            orig_name = split_method(m)[0]
            sig = f"{orig_name}({args}) : {ret_display}" if ret_display != "void" else f"{orig_name}({args})"
            sig = fix_html_entities(sig)
            
            md_lines.append(f"| {member_idx} | {sig} | {desc} |")
            member_idx += 1
            
        for p in sorted(list(members['properties'])):
            p_safe = fix_html_entities(p)
            prop_desc = generate_property_desc(p, layer)
            md_lines.append(f"| {member_idx} | {p_safe} | {prop_desc} |")
            member_idx += 1
            
        md_lines.append("")
        class_idx += 1
        
with open(dest_file, "w", encoding="utf-8") as f:
    f.write("\n".join(md_lines))

print(f"Markdown generated successfully at {dest_file}")
