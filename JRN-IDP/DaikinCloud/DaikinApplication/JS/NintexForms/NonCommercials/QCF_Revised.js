function getCssClassName() {
    let cssClass = "";
    const vendor1 = String(NWF$(".v1 input").val());
    const vendor2 = String(NWF$(".v2 input").val());
    const vendor3 = String(NWF$(".v3 input").val());
    const vendorAppointed = String(NWF$(".vApp input").val());

    if (vendorAppointed.toUpperCase() === vendor1.toUpperCase()) cssClass = ".nf-form-input.v1";
    else if (vendorAppointed.toUpperCase() === vendor2.toUpperCase()) cssClass = ".nf-form-input.v2";
    else if (vendorAppointed.toUpperCase() === vendor3.toUpperCase()) cssClass = ".nf-form-input.v3";

    return cssClass;
};

function changeBackgroundColor(cssSelector, color) {
    NWF$(cssSelector).css('background-color', color);
};

function disableControlWhenApproval() {
    const approvalStatus = String(NWF$(".approvalStatus input").val());
    console.log(approvalStatus);
    if (approvalStatus === "4") NWF$(".rev-amount input").prop('readonly', true);
};

NWF$(function () {
    const btnApproval = NWF$(".btnApproval input[type='radio']");
    btnApproval.prop('checked', false);
    btnApproval.trigger('change');

    NWF$(".comment textarea").val("");

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

    const className = getCssClassName();
    if (className) changeBackgroundColor(className, "grey");

    disableControlWhenApproval();
});
