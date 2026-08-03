/**
 * Toast Queue Manager for SweetAlert2 (FIFO)
 * Ensures multiple real-time notifications are displayed sequentially one-by-one.
 */
(function (window) {
    const queue = [];
    let isShowing = false;

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

        const displayTitle = truncateText(noti.title, 255);
        const displayContent = truncateText(noti.content, 100);
        const defaultTargetUrl = options.defaultTargetUrl || '/Notification';
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
                toast.addEventListener('click', (e) => {
                    if (e.target.tagName === 'BUTTON') return;
                    e.preventDefault();

                    const targetUrl = noti.linkAction || defaultTargetUrl;
                    try {
                        const urlObj = new URL(targetUrl, window.location.origin);
                        if (urlObj.pathname.toLowerCase() === window.location.pathname.toLowerCase()) {
                            window.location.href = targetUrl;
                            window.location.reload();
                        } else {
                            window.location.href = targetUrl;
                        }
                    } catch (err) {
                        window.location.href = targetUrl;
                    }
                });
            },
            didClose: () => {
                isShowing = false;
                // Short delay before displaying next toast in queue
                setTimeout(processQueue, 300);
            }
        });

        Toast.fire({
            icon: noti.icon || 'info',
            title: displayTitle,
            html: `
                <div class="text-left font-body">
                    <p class="text-[12px] text-slate-600 mt-1 leading-relaxed">${displayContent}</p>
                    ${noti.linkAction ? `<a href="${noti.linkAction}" class="inline-block mt-2 text-[12px] font-bold ${accentColorClass} transition-colors">View Detail &rarr;</a>` : ''}
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
