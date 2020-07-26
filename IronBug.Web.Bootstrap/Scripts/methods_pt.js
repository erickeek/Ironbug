/*
* Localized default methods for the jQuery validation plugin.
* Locale: PT_BR
*/
jQuery.extend(jQuery.validator.methods, {
    date: function(value, element) {
        return this.optional(element) || /^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((19|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((19|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((19|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$/.test(value);
    },
    number: function(value, element) {
        return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$/.test(value);
    },
    cpf: function(valor) {
        var valido = true;
        var vsCpf = valor.replace(/[.]/g, "").replace(/[-]/g, "");
        if (vsCpf.length != 11)
            valido = false;
        var vbIgual = true;
        for (var i = 1; i < 11; i++) {
            if (vsCpf.charAt(i) != vsCpf.charAt(0)) {
                vbIgual = false;
                break;
            }
        }
        if (vbIgual == true || vsCpf == "12345678909")
            valido = false;
        var vaNumeros = new Array();
        for (var i = 0; i < 11; i++)
            vaNumeros[i] = parseInt(vsCpf.charAt(i));
        var vnSoma = 0;
        for (var i = 0; i < 9; i++)
            vnSoma += (10 - i) * vaNumeros[i];
        var vnResultado = vnSoma % 11;
        if (vnResultado == 1 || vnResultado == 0) {
            if (vaNumeros[9] != 0)
                valido = false;
        } else if (vaNumeros[9] != 11 - vnResultado)
            valido = false;
        vnSoma = 0;
        for (var i = 0; i < 10; i++)
            vnSoma += (11 - i) * vaNumeros[i];
        vnResultado = vnSoma % 11;
        if (vnResultado == 1 || vnResultado == 0) {
            if (vaNumeros[10] != 0)
                valido = false;
        } else if (vaNumeros[10] != 11 - vnResultado)
            valido = false;
        if (valido == false) {
            return false;
        }
        return valido;
    },
    cnpj: function(valor) {
        var cnpj = valor;
        var valida = new Array(6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2);
        var dig1 = new Number;
        var dig2 = new Number;

        var exp = /\.|\-|\//g;
        cnpj = cnpj.toString().replace(exp, "");
        var digito = new Number(eval(cnpj.charAt(12) + cnpj.charAt(13)));

        for (var i = 0; i < valida.length; i++) {
            dig1 += (i > 0 ? (cnpj.charAt(i - 1) * valida[i]) : 0);
            dig2 += cnpj.charAt(i) * valida[i];
        }
        dig1 = (((dig1 % 11) < 2) ? 0 : (11 - (dig1 % 11)));
        dig2 = (((dig2 % 11) < 2) ? 0 : (11 - (dig2 % 11)));

        if (((dig1 * 10) + dig2) != digito) {
            return false;
        }
        return true;
    }
});
jQuery.validator.classRuleSettings.cpf = { cpf: true };
jQuery.validator.classRuleSettings.cnpj = { cnpj: true };