NWF$(function () {
    NWF$('.number').blur(function () {
        if (NWF$(this).val().length > 0) {
            NWF$(this).val(NWF$(this).val().replace(/,/g, ''));
            var amount = NWF$(this).val();
            NWF$(this).val(addCommas(amount));
        }
    });

    NWF$('.number').keypress(function (evt) {
        OnlyNumberAlsoDecimals(evt)
    });
    NWF$('.btnApproval input[type="radio"]').prop('checked', false); NWF$('#' + Comment).val("");

    const control = NWF$("#" + Approvers);
    control.val('Approver1; Approver2;');
    control.trigger('change');
});