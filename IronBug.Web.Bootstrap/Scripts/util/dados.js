(function () {

    var estados = [
        { Nome: "Acre", Uf: "AC" },
        { Nome: "Alagoas", Uf: "AL" },
        { Nome: "Amapá", Uf: "AP" },
        { Nome: "Amazonas", Uf: "AM" },
        { Nome: "Bahia", Uf: "BA" },
        { Nome: "Ceará", Uf: "CE" },
        { Nome: "Distrito Federal", Uf: "DF" },
        { Nome: "Espírito Santo", Uf: "ES" },
        { Nome: "Goiás", Uf: "GO" },
        { Nome: "Maranhão", Uf: "MA" },
        { Nome: "Mato Grosso", Uf: "MT" },
        { Nome: "Mato Grosso do Sul", Uf: "MS" },
        { Nome: "Minas Gerais", Uf: "MG" },
        { Nome: "Pará", Uf: "PA" },
        { Nome: "Paraíba", Uf: "PB" },
        { Nome: "Paraná", Uf: "PR" },
        { Nome: "Pernambuco", Uf: "PE" },
        { Nome: "Piauí", Uf: "PI" },
        { Nome: "Rio de Janeiro", Uf: "RJ" },
        { Nome: "Rio Grande do Norte", Uf: "RN" },
        { Nome: "Rio Grande do Sul", Uf: "RS" },
        { Nome: "Rondônia", Uf: "RO" },
        { Nome: "Roraima", Uf: "RR" },
        { Nome: "Santa Catarina", Uf: "SC" },
        { Nome: "São Paulo", Uf: "SP" },
        { Nome: "Sergipe", Uf: "SE" },
        { Nome: "Tocantins", Uf: "TO" }
    ];

    window.Dados = {
        estados: function () {
            return $.deepClone(estados);
        }
    };
})();