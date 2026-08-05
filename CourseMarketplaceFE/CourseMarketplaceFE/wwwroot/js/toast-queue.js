/**
 * Toast Queue Manager for SweetAlert2 (FIFO)
 * Ensures multiple real-time notifications are displayed sequentially one-by-one.
 */
(function (window) {
    const queue = [];
    let isShowing = false;

    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    function decodeJwtToken(token) {
        if (!token) return null;
        try {
            const base64Url = token.split('.')[1];
            if (!base64Url) return null;
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            return JSON.parse(jsonPayload);
        } catch (e) {
            return null;
        }
    }

    function getCurrentUserId() {
        const token = (typeof window !== 'undefined' && window.ADMIN_LAYOUT_TOKEN)
            || (typeof window !== 'undefined' && window.INSTRUCTOR_LAYOUT_TOKEN)
            || (typeof window !== 'undefined' && window.LEARNER_LAYOUT_TOKEN)
            || (typeof TOKEN !== 'undefined' ? TOKEN : null)
            || (typeof TOKEN_USER !== 'undefined' ? TOKEN_USER : null)
            || getCookie("AccessToken")
            || getCookie(".AspNetCore.Identity.Application");

        if (!token) return null;
        const decoded = decodeJwtToken(token);
        if (!decoded) return null;
        const idStr = decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || decoded["nameid"] || decoded["sub"];
        return idStr ? parseInt(idStr) : null;
    }

    function showNotificationDetailModal(noti) {
        isShowing = true;
        const rawDate = noti.createdAt || noti.CreatedAt;
        const createdAtDate = rawDate ? new Date(rawDate) : new Date();
        const formattedDate = !isNaN(createdAtDate.getTime())
            ? createdAtDate.toLocaleString('en-US', {
                hour: '2-digit', minute: '2-digit',
                day: '2-digit', month: '2-digit', year: 'numeric',
                hour12: false
              })
            : '';

        const escapeHtml = (str) => {
            if (!str) return '';
            return str.replace(/&/g, "&amp;")
                      .replace(/</g, "&lt;")
                      .replace(/>/g, "&gt;")
                      .replace(/"/g, "&quot;")
                      .replace(/'/g, "&#039;");
        };

        const title = escapeHtml(noti.title || noti.Title || 'Notification Details');
        const content = escapeHtml(noti.content || noti.Content || '');
        const linkAction = noti.linkAction || noti.LinkAction;

        Swal.fire({
            title: `<div class="flex items-center gap-2 text-teal-700 text-base font-bold uppercase tracking-wider" style="text-align: left;">
                        <span class="material-symbols-outlined text-xl">info</span>
                        <span>Notification Details</span>
                    </div>`,
            html: `
                <div class="text-left space-y-4 pt-2 font-sans" style="text-align: left;">
                    <div>
                        <h3 class="text-xl font-bold text-slate-800 leading-tight break-all mb-1">${title}</h3>
                        ${formattedDate ? `
                            <div class="flex items-center gap-1 text-slate-400 text-xs uppercase tracking-tight font-medium mt-1">
                                <span class="material-symbols-outlined text-[14px]">schedule</span>
                                <span>${formattedDate}</span>
                            </div>
                        ` : ''}
                    </div>
                    <div class="bg-slate-50 p-5 rounded-2xl border border-slate-100 mt-3" style="background-color: #f8fafc; padding: 1.25rem; border-radius: 1rem; border: 1px solid #f1f5f9;">
                        <p class="text-slate-600 leading-relaxed text-sm whitespace-pre-wrap break-words m-0" style="color: #475569; font-size: 0.875rem; line-height: 1.6; margin: 0;">${content}</p>
                    </div>
                    ${linkAction ? `
                        <div class="pt-2" style="padding-top: 0.5rem;">
                            <a href="${linkAction}" class="inline-flex items-center gap-2 px-5 py-2.5 bg-teal-600 hover:bg-teal-700 text-white font-bold rounded-xl shadow-md hover:shadow-lg transition-all text-xs text-decoration-none" style="display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.625rem 1.25rem; background-color: #0d9488; color: white; border-radius: 0.75rem; font-weight: 700; font-size: 0.75rem; text-decoration: none;" onclick="Swal.close()">
                                <span class="material-symbols-outlined text-base">open_in_new</span>
                                View Details
                            </a>
                        </div>
                    ` : ''}
                </div>
            `,
            showConfirmButton: true,
            confirmButtonText: 'Close',
            confirmButtonColor: '#64748b',
            customClass: {
                popup: 'rounded-3xl shadow-2xl border border-slate-100 p-6',
                title: 'p-0 text-left border-b border-slate-100 pb-4',
                htmlContainer: 'p-0 text-left',
                confirmButton: 'px-6 py-2.5 bg-white border border-slate-200 text-slate-600 font-bold rounded-xl hover:bg-slate-100 transition-all text-sm shadow-none'
            },
            background: '#ffffff',
            width: '520px',
            didClose: () => {
                isShowing = false;
                setTimeout(processQueue, 300);
            }
        });
    }

    function enqueueToast(noti, options = {}) {
        if (!noti || !noti.title) return;
        queue.push({ noti, options });
        if (!isShowing) {
            processQueue();
        }
    }

    function processQueue() {
        if (queue.length === 0) {
            isShowing = false;
            return;
        }

        if (typeof Swal === 'undefined') {
            console.warn('SweetAlert2 (Swal) is not loaded.');
            isShowing = false;
            return;
        }

        isShowing = true;
        const item = queue.shift();
        const noti = item.noti;
        const options = item.options || {};

        const truncateText = (text, maxLength = 100) => {
            if (!text) return "";
            return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
        };

        const displayTitle = truncateText(noti.title || noti.Title, 255);
        const displayContent = truncateText(noti.content || noti.Content, 100);
        const accentColorClass = options.accentColorClass || 'text-emerald-600 hover:text-emerald-700';

        const Toast = Swal.mixin({
            toast: true,
            position: options.position || 'top-end',
            showConfirmButton: false,
            timer: options.timer || 7000,
            timerProgressBar: true,
            width: options.width || '600px',
            padding: options.padding || '24px',
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);

                toast.style.cursor = 'pointer';
                toast.addEventListener('click', async (e) => {
                    if (e.target.tagName === 'BUTTON') return;
                    e.preventDefault();

                    const currentUserId = getCurrentUserId();
                    const notiId = noti.notificationId || noti.id || noti.NotificationId;
                    const recId = noti.receiverId || noti.ReceiverId;
                    const isRead = noti.isRead !== undefined ? noti.isRead : noti.IsRead;

                    if (currentUserId && recId === currentUserId && !isRead && notiId) {
                        try {
                            const response = await fetch(`/Notification/MarkAsRead/${notiId}`, { method: 'PUT' });
                            if (response.ok) {
                                noti.isRead = true;
                                noti.IsRead = true;
                                document.dispatchEvent(new CustomEvent("ReceiveNotificationEvent"));
                            }
                        } catch (err) {
                            console.error("Failed to mark notification as read from toast:", err);
                        }
                    }

                    showNotificationDetailModal(noti);
                });
            },
            didClose: () => {
                if (!Swal.isVisible()) {
                    isShowing = false;
                    setTimeout(processQueue, 300);
                }
            }
        });

        const linkAction = noti.linkAction || noti.LinkAction;

        Toast.fire({
            icon: noti.icon || 'info',
            title: displayTitle,
            html: `
                <div class="text-left font-body">
                    <p class="text-[12px] text-slate-600 mt-1 leading-relaxed">${displayContent}</p>
                    ${linkAction ? `<a href="javascript:void(0)" class="inline-block mt-2 text-[12px] font-bold ${accentColorClass} transition-colors">View Detail &rarr;</a>` : ''}
                </div>
            `
        });
    }

    // Expose globally
    window.ToastQueue = {
        enqueue: enqueueToast,
        getPendingCount: () => queue.length,
        clear: () => {
            queue.length = 0;
            isShowing = false;
        }
    };
})(window);
