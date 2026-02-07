"use strict";

const notificationConnection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

notificationConnection.start().then(function () {
    console.log("Notification Hub Connected");
}).catch(function (err) {
    return console.error(err.toString());
});

notificationConnection.on("ReceiveNotification", function (notification) {
    // Normalize properties
    const title = notification.title || notification.Title;
    const message = notification.message || notification.Message;
    const link = notification.link || notification.Link;

    // 1. Update Badge
    updateNotificationBadge(1, true); // Increment by 1

    // 2. Add to Dropdown List
    const list = document.getElementById("notificationList");
    if (list) {
        const noNotifs = list.querySelector(".text-muted.text-center");
        if (noNotifs) noNotifs.remove();

        const newItem = document.createElement("div");
        newItem.className = "list-group-item list-group-item-action bg-light";
        newItem.innerHTML = `
            <div class="d-flex w-100 justify-content-between align-items-start">
                <div class="flex-grow-1">
                    <div class="d-flex align-items-center mb-1">
                        <span class="badge bg-primary me-2">New</span>
                        <h6 class="mb-0 text-sm fw-bold">${title}</h6>
                    </div>
                    <p class="mb-1 text-xs text-muted text-truncate">${message}</p>
                    <small class="text-muted text-xs">Just now</small>
                </div>
            </div>
            <a href="${link}" class="stretched-link"></a>
        `;
        list.insertBefore(newItem, list.firstChild);
    }

    // 3. Show Toast (Optional - premium feel)
    showToast(title, message);
});

function updateNotificationBadge(count, isIncrement = false) {
    const badge = document.getElementById("notificationBadge");
    if (badge) {
        let current = parseInt(badge.innerText) || 0;
        let newValue = isIncrement ? current + count : count;

        if (newValue > 0) {
            badge.innerText = newValue;
            badge.classList.remove("d-none");
        } else {
            badge.classList.add("d-none");
        }
    }
}

function showToast(title, message) {
    // Check if toast container exists, if not create one
    let container = document.getElementById("toast-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "toast-container";
        container.className = "toast-container position-fixed bottom-0 end-0 p-3";
        document.body.appendChild(container); // Append to body, not document
    }

    const toastId = "toast-" + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white bg-primary border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong><br/>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    // Create a temporary element to parse the HTML string
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = toastHtml;
    const toastElement = tempDiv.firstElementChild;

    container.appendChild(toastElement);

    const toast = new bootstrap.Toast(toastElement);
    toast.show();
}
