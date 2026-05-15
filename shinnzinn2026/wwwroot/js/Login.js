// 🌟 1. 入力用モーダルを開く処理
function openModal(actionType) {
    var modal = document.getElementById('authModal');
    var title = document.getElementById('modalTitle');
    var form = document.getElementById('modalForm');

    if (actionType === 'CheckIn') {
        title.innerText = '出勤打刻';
        title.style.color = '#96c93d'; // 出勤は緑
        form.action = '/Login/CheckIn';
    } else {
        title.innerText = '退勤打刻';
        title.style.color = '#ff512f'; // 退勤は赤
        form.action = '/Login/CheckOut';
    }
    modal.style.display = 'flex';
}

// 🌟 2. 入力用モーダルを閉じる処理
function closeModal() {
    document.getElementById('authModal').style.display = 'none';
}

// 🌟 3. 結果モーダルを表示して、文字を書き換える処理
function showResultModal(successMsg, errorMsg) {
    var modal = document.getElementById('resultModal');
    var title = document.getElementById('resultTitle');
    var text = document.getElementById('resultText');

    if (successMsg) {
        title.innerText = '打刻完了！';
        title.style.color = '#96c93d';
        text.innerText = successMsg;
    } else if (errorMsg) {
        title.innerText = 'エラー';
        title.style.color = '#ff512f';
        text.innerText = errorMsg;
    }
    modal.style.display = 'flex';
}

// 🌟 4. 結果モーダルを閉じる処理
function closeResultModal() {
    document.getElementById('resultModal').style.display = 'none';
}


// 🌟 5. 【超重要】画面が読み込まれた瞬間に実行される処理
document.addEventListener("DOMContentLoaded", function () {
    var modal = document.getElementById('resultModal');
    if (modal) {
        // HTMLの data-success と data-error から文字を読み取る
        var successMsg = modal.getAttribute('data-success');
        var errorMsg = modal.getAttribute('data-error');

        // どちらかに文字が入っていれば、結果モーダルをドーンと表示！
        if (successMsg || errorMsg) {
            showResultModal(successMsg, errorMsg);
        }
    }
});