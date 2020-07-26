(function () {
    const app = angular.module("app");

    app.service("$enums", function ($enumsHelper) {
        return $enumsHelper({

        });
    });

    app.constant("$appInfo", {
        version: "?v=1.0.0"
    });

    app.directive("messages", function ($messages) {
        return {
            restrict: "E",
            link: function () {
                const data = getCookie("Mensagem");
                if (data != null && data != "") {

                    if (data.Status)
                        $messages.success(data.Mensagem);
                    else
                        $messages.error(data.Mensagem);

                    setCookie("Mensagem", null, 0);
                }
            }
        };
    });
})();
