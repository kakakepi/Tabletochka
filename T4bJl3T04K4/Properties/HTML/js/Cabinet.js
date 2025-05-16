document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('profileForm');
    form.addEventListener('submit', handleSubmit);

    document.getElementById('uploadBtn').addEventListener('click', uploadPhoto);
    document.getElementById('deleteBtn')?.addEventListener('click', deletePhoto);
    document.getElementById('deleteAccountBtn').addEventListener('click', deleteAccount);

    document.getElementById('cancelBtn').addEventListener('click', resetForm);

    window.chrome.webview.addEventListener('message', event => {
        if (event.data.type === 'success') {
            alert(event.data.message);
            location.reload();
        }
        if (event.data.type === 'error') {
            alert(`Ошибка: ${event.data.message}`);
            location.reload();

        }
    });
});

function handleSubmit(e) {
    e.preventDefault();

    const data = {
        action: "updateProfile",
        id: window.currentUserId,
        username: document.getElementById('username').value,
        lastname: document.getElementById('lastname').value,
        firstname: document.getElementById('firstname').value,
        gender: document.getElementById('gender').value,
        birthdate: document.getElementById('birthdate').value,
        oldPassword: document.getElementById('oldPassword').value,
        newPassword: document.getElementById('newPassword').value
    };

    window.chrome.webview.postMessage(data);
}

function resetForm() {
    document.getElementById('profileForm').reset();
}

function uploadPhoto() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';

    input.onchange = e => {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = event => {
                window.chrome.webview.postMessage({
                    action: "uploadPhoto",
                    file: event.target.result.split(',')[1]
                });
            };
            reader.readAsDataURL(file);
            location.reload();

        }
    };

    input.click();

}

function deletePhoto() {
    if (confirm('Вы уверены, что хотите удалить фото?')) {
        window.chrome.webview.postMessage({ action: "deletePhoto" });
        location.reload();

    }
}

function deleteAccount() {
    if (confirm('Вы действительно хотите удалить свою учётную запись? Это действие необратимо.')) {
        window.chrome.webview.postMessage({ action: "deleteAccount" });
    }
}

window.chrome.webview.addEventListener('message', event => {
    const user = event.data;
    window.currentUserId = user.id;

    document.getElementById('username').value = user.username || '';
    document.getElementById('lastname').value = user.lastname || '';
    document.getElementById('firstname').value = user.firstname || '';
    document.getElementById('gender').value = user.gender ? 'male' : 'female';
    document.getElementById('birthdate').value = user.dateOfBirth || '';

    if (event.data.type === 'photo-updated') {
        document.querySelector('.profile-pic img').src = event.data.picture || 'images/default-avatar.jpg';
    }
    if (event.data.type === 'photo-deleted') {
        document.querySelector('.profile-pic img').src = 'images/default-avatar.jpg';
    }

    if (user.picture) {
        document.getElementById('profileImage').src = `data:image/jpeg;base64,${user.picture}`;
    }
});
function togglePanel() {
  const overlay = document.querySelector('.overlay');
  const panel = document.getElementById('sidePanel');
  if (overlay.style.display === 'block') {
    overlay.style.display = 'none';
    panel.style.right = '-300px';
  } else {
    overlay.style.display = 'block';
    panel.style.right = '0';
  }
}

function toggleSubmenu(id) {
  const submenu = document.getElementById(id);
  submenu.style.display = submenu.style.display === 'block' ? 'none' : 'block';
}
function logout() {
    window.chrome.webview.postMessage({ action: "logout" });
}
