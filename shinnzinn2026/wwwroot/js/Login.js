// ==========================================
// 🌟 1. モーダル操作用の関数（変更なし：あなたのカスタマイズを維持）
// ==========================================
function openModal(actionType) {
    var modal = document.getElementById('authModal');
    var title = document.getElementById('modalTitle');
    var form = document.getElementById('modalForm');
    if (actionType === 'CheckIn') {
        title.innerText = '出勤';
        title.style.color = '#96c93d';
        form.action = '/Login/CheckIn';
    } else {
        title.innerText = '退勤';
        title.style.color = '#ff512f';
        form.action = '/Login/CheckOut';
    }
    modal.style.display = 'flex';
}

function closeModal() {
    document.getElementById('authModal').style.display = 'none';
}

function showResultModal(successMsg, errorMsg) {
    var modal = document.getElementById('resultModal');
    var title = document.getElementById('resultTitle');
    var text = document.getElementById('resultText');
    if (successMsg) {
        title.innerText = '登録完了しました';
        title.style.color = '#96c93d';
        text.innerText = successMsg;
    } else if (errorMsg) {
        title.innerText = 'エラー';
        title.style.color = '#ff512f';
        text.innerText = errorMsg;
    }
    modal.style.display = 'flex';
}

function closeResultModal() {
    document.getElementById('resultModal').style.display = 'none';
}

// ==========================================
// 🌟 2. リアルタイム時計＆押し出しアニメーション（CSS完全不要版）
// ==========================================
function startClock() {
    const clock = document.getElementById('realtimeClock');
    if (!clock) return;

    // 前回の値を保存して、変わった時だけアニメーションさせる
    let lastTime = { h: "", m: "", s: "" };

    // コロンの点滅アニメーション（JSで直接制御）
    const blinkKeyframes = [
        { opacity: 1 },
        { opacity: 0, offset: 0.5 },
        { opacity: 1 }
    ];
    const blinkTiming = { duration: 1000, iterations: Infinity, easing: 'step-start' };

    setInterval(function () {
        const now = new Date();
        const y = now.getFullYear();
        const mon = ('0' + (now.getMonth() + 1)).slice(-2);
        const d = ('0' + now.getDate()).slice(-2);
        const h = ('0' + now.getHours()).slice(-2);
        const min = ('0' + now.getMinutes()).slice(-2);
        const sec = ('0' + now.getSeconds()).slice(-2);

        const currentTime = { h, m: min, s: sec };

        // 各パーツのHTMLを生成（overflow: hiddenの窓を作る）
        const wrap = (val, id) => `
            <span style="display: inline-block; position: relative; overflow: hidden; height: 1.2em; vertical-align: bottom;">
                <span id="clock-${id}" style="display: inline-block;">${val}</span>
            </span>`;

        // HTMLを一気に書き換え
        clock.innerHTML = `
            <span style="font-size: 0.5em; opacity: 0.8; margin-right: 15px;">${y}/${mon}/${d}</span>
            ${wrap(h, 'h')} <span id="colon1" style="margin: 0 5px;">:</span>
            ${wrap(min, 'm')} <span id="colon2" style="margin: 0 5px;">:</span>
            ${wrap(sec, 's')}
        `;

        // 🌟 変化した数字だけに「押し出しアニメーション」を実行
        ['h', 'm', 's'].forEach(type => {
            if (currentTime[type] !== lastTime[type]) {
                const el = document.getElementById(`clock-${type}`);
                if (el) {
                    el.animate([
                        { transform: 'translateY(-100%)', opacity: 0 },
                        { transform: 'translateY(0)', opacity: 1 }
                    ], {
                        duration: 400,
                        easing: 'cubic-bezier(0.175, 0.885, 0.32, 1.275)'
                    });
                }
            }
        });

        // コロンの点滅を開始（最初の一回だけ実行）
        ['colon1', 'colon2'].forEach(id => {
            const el = document.getElementById(id);
            if (el && !el.getAnimations().length) {
                el.animate(blinkKeyframes, blinkTiming);
            }
        });

        lastTime = currentTime;
    }, 1000);
}

// ==========================================
// 🚀 3. メイン処理
// ==========================================
document.addEventListener("DOMContentLoaded", function () {
    startClock();

    var btnCheckIn = document.getElementById('btnCheckIn');
    var btnCheckOut = document.getElementById('btnCheckOut');
    var btnCloseModal = document.getElementById('btnCloseModal');
    var btnCloseResultModal = document.getElementById('btnCloseResultModal');

    if (btnCheckIn) btnCheckIn.addEventListener('click', () => openModal('CheckIn'));
    if (btnCheckOut) btnCheckOut.addEventListener('click', () => openModal('CheckOut'));
    if (btnCloseModal) btnCloseModal.addEventListener('click', closeModal);
    if (btnCloseResultModal) btnCloseResultModal.addEventListener('click', closeResultModal);

    var resultModal = document.getElementById('resultModal');
    if (resultModal) {
        var successMsg = resultModal.getAttribute('data-success');
        var errorMsg = resultModal.getAttribute('data-error');
        if (successMsg || errorMsg) showResultModal(successMsg, errorMsg);
    }
});