// wwwroot/js/login.js

function openModal(actionType) {
    var modal = document.getElementById('authModal');
    var title = document.getElementById('modalTitle');
    var form = document.getElementById('modalForm');

    // 押されたボタンによってタイトルと送信先を切り替える
    if (actionType === 'CheckIn') {
        title.innerText = '出勤';
        title.style.color = '#96c93d'; // 出勤は緑
        form.action = '/Login/CheckIn';
    } else {
        title.innerText = '退勤';
        title.style.color = '#ff512f'; // 退勤は赤
        form.action = '/Login/CheckOut';
    }

    // モーダルを表示
    modal.style.display = 'flex';
}

function closeModal() {
    // モーダルを非表示
    document.getElementById('authModal').style.display = 'none';
}