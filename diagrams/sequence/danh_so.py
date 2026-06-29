import re
import sys
from typing import List, Optional


class PlantUMLRenumberer:
    def __init__(self):
        self.lines: List[str] = []
        self.stack: List[str] = []
        self.block_stack: List[str] = []
        self.alt_roots: List[Optional[str]] = []
        self.last_main_number: str = "0"
        self.pending_branch: Optional[int] = None
        self.branch_started: bool = False
        self.main_ready: bool = False
        self.just_entered_else: bool = False  # Flag for else branch start
        self.expect_sub: bool = False  # Flag to force sub-level after first message in branch
        self.ref_active: bool = False  # Flag for being inside ref block
        self.opt_id = 0  # Counter for sequential opt branches

    def _clean_label(self, msg: str) -> str:
        return re.sub(r'^\s*\d+(?:\.\d+)*\s*\.?\s*', '', msg).strip()

    def _get_next_main_number(self) -> str:
        base = int(self.last_main_number)
        base += 1
        self.last_main_number = str(base)
        return str(base)

    def _start_branch(self, branch_id: int):
        if not self.stack:
            base = self.last_main_number
        else:
            base = self.stack[-1]
        root = f"{base}.{branch_id}"
        self.stack.append(root)
        self.branch_started = False
        self.expect_sub = False  # Reset for new branch

    def _increment_in_current_branch(self) -> str:
        if not self.stack:
            if self.main_ready:
                self.main_ready = False
                return self.last_main_number
            return self._get_next_main_number()

        if not self.branch_started:
            self.branch_started = True
            return self.stack[-1]

        # Force sub-level for first increment after branch start message
        if self.expect_sub:
            self.expect_sub = False
            base = self.stack[-1]
            new_num = f"{base}.1"
            self.stack[-1] = new_num
            self.branch_started = True  # Keep for normal increments
            return new_num

        # Normal increment
        parts = self.stack[-1].split('.')
        parts[-1] = str(int(parts[-1]) + 1)
        new_num = '.'.join(parts)
        self.stack[-1] = new_num
        return new_num

    def process(self, input_lines: List[str]) -> List[str]:
        self.lines.clear()
        self.stack.clear()
        self.block_stack.clear()
        self.alt_roots.clear()
        self.last_main_number = "0"
        self.pending_branch = None
        self.branch_started = False
        self.main_ready = False
        self.just_entered_else = False
        self.expect_sub = False
        self.ref_active = False
        self.opt_id = 0  # Counter for sequential opt branches

        for line in input_lines:
            stripped = line.strip()
            original_line = line

            if not stripped or stripped.startswith((
                '@startuml', '@enduml', 'hide', 'mainframe',
                'actor ', 'participant ', 'database ',
                '<style>', '</style>', 'style', 'activate'
            )):
                self.lines.append(original_line)
                continue

            if stripped.startswith('alt '):
                label = self._clean_label(stripped[4:])
                self.lines.append(f"alt {label}")
                self.pending_branch = 1
                self.block_stack.append('alt')
                self.alt_roots.append(None)
                continue

            if stripped.startswith('else '):
                label = self._clean_label(stripped[5:])
                self.lines.append(f"else {label}")
                if self.block_stack and self.block_stack[-1] == 'alt' and self.alt_roots and self.alt_roots[-1]:
                    alt_root = self.alt_roots[-1]
                    base_parts = alt_root.split('.')[:-1]
                    base = '.'.join(base_parts)
                    new_root = f"{base}.2"
                    self.stack[-1] = new_root
                self.pending_branch = None
                self.branch_started = False
                self.just_entered_else = True
                continue

            if stripped.startswith('loop '):
                rest = stripped[5:].strip()
                label = self._clean_label(rest)
                self.lines.append(f"loop {label}")
                self.block_stack.append('loop')
                continue

            if stripped.startswith('opt ') or stripped.startswith('group opt '):
                if stripped.startswith('group opt '):
                    rest = stripped[10:].strip()
                    prefix = 'group opt'
                else:
                    rest = stripped[4:].strip()
                    prefix = 'opt'
                label = self._clean_label(rest)
                self.lines.append(f"{prefix} {label}")
                self.opt_id += 1
                self.pending_branch = self.opt_id  # Sequential branch id for opts
                self.block_stack.append('opt')
                continue

            if stripped == 'end':
                self.lines.append("end")
                popped = None
                if self.block_stack:
                    popped = self.block_stack.pop()
                    if popped == 'alt':
                        alt_root = self.alt_roots.pop() if self.alt_roots else None
                        if self.stack:
                            self.stack.pop()
                        if not self.stack and alt_root:
                            main_part = alt_root.split('.')[0]
                            self.last_main_number = str(int(main_part) + 1)
                            self.main_ready = True
                        # Force increment for next message after end alt
                        self.branch_started = True
                        # Reset expect_sub to prevent leak from inner branches
                        self.expect_sub = False
                    elif popped == 'opt':
                        # For opt, pop stack like alt (single branch)
                        if self.stack:
                            self.stack.pop()
                        self.branch_started = True
                        self.expect_sub = False
                self.pending_branch = None
                continue

            # Handle ref over
            if stripped.startswith('ref over '):
                self.lines.append(original_line)
                self.ref_active = True
                continue

            if stripped == 'end ref':
                self.lines.append("end ref")
                self.ref_active = False
                continue

            # Handle messages and refs
            is_ref = False  # No longer needed as ref over handled separately
            arrows = ['->', '-->', '->>', '-->>', '<->', '<--', '<-', '++', '--']
            is_message = any(op in stripped for op in arrows)
            is_ref_text = self.ref_active and stripped.strip() and not stripped.startswith(('ref over', 'end ref', 'end', 'alt', 'else', 'loop', 'opt', 'group opt'))

            if is_message or is_ref_text:
                if is_message:
                    colon_idx = line.find(':')
                    if colon_idx == -1:
                        self.lines.append(original_line)
                        continue
                    raw_msg = line[colon_idx + 1:].strip()
                    clean_msg = self._clean_label(raw_msg)
                    indent = line[:colon_idx + 1]
                else:  # is_ref_text
                    raw_msg = stripped
                    clean_msg = self._clean_label(raw_msg)
                    # Preserve indentation
                    indent_len = len(line) - len(stripped)
                    indent = line[:indent_len]

                was_new_branch = self.pending_branch is not None or self.just_entered_else

                if self.pending_branch is not None:
                    self._start_branch(self.pending_branch)
                    self.pending_branch = None
                    if self.alt_roots and self.block_stack and self.block_stack[-1] == 'alt':
                        self.alt_roots[-1] = self.stack[-1]

                number = self._increment_in_current_branch()
                self.last_main_number = number.split('.')[0]

                label = f"{number}. {clean_msg}" if clean_msg else f"{number}."

                if is_message:
                    new_line = indent + label
                else:  # ref text
                    new_line = indent + label

                self.lines.append(new_line)

                # After first message in new branch, expect sub-level for subsequent messages
                if was_new_branch and self.stack:
                    self.expect_sub = True
                    if self.just_entered_else:
                        self.just_entered_else = False

                continue

            self.lines.append(original_line)

        return self.lines


def main(input_path: str, output_path: Optional[str] = None):
    try:
        with open(input_path, 'r', encoding='utf-8') as f:
            lines = [x.rstrip('\n') for x in f.readlines()]

        renumberer = PlantUMLRenumberer()
        processed = renumberer.process(lines)

        out_path = output_path or input_path
        with open(out_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(processed) + '\n')

        print(f"✅ Đánh số hoàn chỉnh theo phân cấp: {out_path}")
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        sys.exit(1)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("📌 Cách dùng: python danh_so.py <input.puml> [output.puml]")
        sys.exit(1)

    input_file = sys.argv[1]
    output_file = sys.argv[2] if len(sys.argv) > 2 else None
    main(input_file, output_file)