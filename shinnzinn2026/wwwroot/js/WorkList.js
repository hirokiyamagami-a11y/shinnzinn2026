// 当日の状態（遅刻など）の表示切り替え
function toggleDailyStatus(day, status) {
    var ids = ['lateReason_', 'earlyReason_', 'absenceReason_'];
    ids.forEach(function (id) {
        var el = document.getElementById(id + day);
        if (el) el.style.display = 'none';
    });
    if (status) {
        var target = document.getElementById(status + 'Reason_' + day);
        if (target) target.style.display = 'block';
    }
}

// 休暇設定（有給・特休）の表示切り替え
function toggleLeaveStatus(day, status) {
    var plArea = document.getElementById('paidLeaveArea_' + day);
    var slArea = document.getElementById('specialLeaveArea_' + day);
    if (plArea) plArea.style.display = (status === 'paidLeave') ? 'block' : 'none';
    if (slArea) slArea.style.display = (status === 'specialLeave') ? 'block' : 'none';
}