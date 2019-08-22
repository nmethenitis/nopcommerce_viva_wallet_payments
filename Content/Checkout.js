$(document).ready(function () {
    $("#txtExpYear").change(function () {
        var value = $(this).val();
        if (value.length == 2) {
            $("#txtExpYear").val("20" + value);
        };
    });

    $("#txtCardNumber").change(function () {
        var value = $(this).val();
        value = value.replace(/\s/g, '');
        value = value.replace(/-/g, '');
        $(this).val(value);
    });

    $("#submit").click(function (e) {
        e.preventDefault();
   });

    VivaPayments.cards.setup({
        publicKey: '1bTBPT+XJlW83esvIjoCcCWLHbXjBhtAR9iV7ye6R8Q=',
        baseURL: 'https://demo.vivapayments.com',
        cardTokenHandler: function (response) {
            if (!response.Error) {
                $("#hidToken").val(response.Token);
                PaymentInfo.save()
            } else {
                alert(response.Error);
            }
        }
    });
});