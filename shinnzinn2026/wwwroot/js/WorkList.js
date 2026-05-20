function toggleStatus(day, status) {
    // 全フィールドを一旦非表示にする
    var fields = ['lateReason', 'earlyReason', 'absenceReason', 'paidLeaveTypes'];
    fields.forEach(function (id) {
        var el = document.getElementById(id + '_' + day);
        if (el) el.style.display = 'none';
    });

    // 選択されたステータスに応じてフィールドを表示
    if (!status) return;
    var targetId = (status === 'paidLeave' ? 'paidLeaveTypes_' : status + 'Reason_') + day;
    var targetEl = document.getElementById(targetId);
    if (targetEl) targetEl.style.display = 'block';
}