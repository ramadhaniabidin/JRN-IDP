var number_input_view = NWF$("div.forms-number .ms-rtestate-field");
number_input_view.each(function () {
    var currTarget = $(this);

    var number_val = currTarget.text();
    if (currTarget != "") {
        number_val = number_val.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        currTarget.text(number_val);
    }

})

var number_input = NWF$("div.forms-number input");
number_input.each(function () {
    var currTarget = $(this);

    var number_val = currTarget.val();
    if (currTarget != "") {
        number_val = number_val.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        currTarget.val(number_val);
    }

    currTarget.on("input", function () {
        var inputValue = currTarget.val();

        // Remove non-numeric characters except for periods and commas
        inputValue = inputValue.replace(/[^0-9.]/g, "");

        var decimalCount = (inputValue.match(/\./g) || []).length;
        if (decimalCount > 1) {
            var parts = inputValue.split(".");
            inputValue = parts.shift() + "." + parts.join("");
        }

        // Format the number with commas for thousands
        var parts = inputValue.split(".");

        parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");

        inputValue = parts.join(".");

        currTarget.val(inputValue);
    });

});