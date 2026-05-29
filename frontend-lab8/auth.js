// auth.js — shared authentication helpers
// Called on every page to gate access and show username in nav.

function requireAuth(callback) {
    const xhr = new XMLHttpRequest();
    xhr.open('GET', BASE_URL + '/api/auth/status', true);
    xhr.withCredentials = true;
    xhr.onload = function () {
        if (this.status === 200) {
            const data = JSON.parse(this.responseText);
            if (data.authenticated) {
                const navUser = document.getElementById('navUser');
                const logoutBtn = document.getElementById('logoutBtn');
                if (navUser) navUser.textContent = '👤 ' + data.username;
                if (logoutBtn) logoutBtn.style.display = 'inline-block';
                if (callback) callback();
            } else {
                window.location.href = 'login.html';
            }
        } else {
            window.location.href = 'login.html';
        }
    };
    xhr.onerror = function () {
        document.body.innerHTML = '<p style="color:red;padding:20px">Cannot reach backend at <strong>' +
            BASE_URL + '</strong>. Is it running?</p>';
    };
    xhr.send();
}

function logout() {
    if (!confirm('Log out?')) return;
    const xhr = new XMLHttpRequest();
    xhr.open('POST', BASE_URL + '/api/auth/logout', true);
    xhr.withCredentials = true;
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.onload = function () { window.location.href = 'login.html'; };
    xhr.send();
}
