const number_input_view = NWF$("div.forms-number .ms-rtestate-field");
number_input_view.each(function () {
    const currTarget = $(this);
    const number_val = currTarget.text().trim();
    if (number_val !== "") {
        currTarget.text(formatNumberWithCommas(number_val));
    }

});


function formatNumberWithCommas(x) {
    if (x === "") return x;
    const parts = x.split(".");
    parts[0] = Number(parts[0]).toLocaleString("en-US");
    return parts.join(".");
};

const number_input = NWF$("div.forms-number input");
number_input.each(function () {
    const currTarget = $(this);
    const number_val = currTarget.val();

    if (currTarget !== "") {
        currTarget.val(formatNumberWithCommas(number_val));
    }

    currTarget.on("input", function () {
        let inputValue = currTarget.val();

        inputValue = inputValue.replace(/[^0-9.]/g, "");

        const decimalCount = (inputValue.match(/\./g) || []).length;
        if (decimalCount > 1) {
            const parts = inputValue.split(".");
            inputValue = parts.shift() + "." + parts.join("");
        }

        currTarget.val(formatNumberWithCommas(inputValue));
    });

});