$.validator.unobtrusive.adapters.add('dynamicrange', ['minvalueproperty', 'maxvalueproperty'],
        function (options) {
            options.rules['dynamicrange'] = options.params;
            if (options.message != null) {
                $.validator.messages.dynamicrangeformat = options.message;
            }
        }
    );

$.validator.addMethod('dynamicrange', function (value, element, params) {
    var minValue = parseInt($('input[name="' + params.minvalueproperty + '"]').val(), 10);
    var maxValue = parseInt($('input[name="' + params.maxvalueproperty + '"]').val(), 10);
    var currentValue = parseInt(value, 10);
    if (isNaN(minValue) || isNaN(maxValue) || isNaN(currentValue) || minValue > currentValue || currentValue > maxValue) {
        $.validator.messages.dynamicrange = $.format($.validator.messages.dynamicrangeformat, minValue, maxValue);
        return false;
    }
    return true;
}, '');