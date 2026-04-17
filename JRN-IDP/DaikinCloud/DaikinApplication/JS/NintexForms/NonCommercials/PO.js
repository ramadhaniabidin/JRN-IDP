NWF$(function () {
    const btnProcessType = NWF$("#" + processType);
    btnProcessType.val("");

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


});