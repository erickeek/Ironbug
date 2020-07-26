(function () {
    var app = angular.module("angular.helpers", []);

    // general
    app.service("$guid", function () {
        return {
            create: function () {
                return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
                    var r = Math.random() * 16 | 0, v = c === "x" ? r : (r & 0x3 | 0x8);
                    return v.toString(16);
                });
            },
            createElementName: function () {
                return "generatedName_" + this.create();
            }
        };
    });
    app.directive("ngBindModel", function ($compile) {
        return {
            compile: function (tEl) {
                tEl[0].removeAttribute("ng-bind-model");
                return function (scope, iEl, iAtr) {
                    iEl[0].setAttribute("ng-model", iAtr.ngBindModel);
                    $compile(iEl[0])(scope);
                }
            }
        }
    })

    // alerts
    app.service("$messages", function ($rootScope) {
        return {
            success: function (text, time) {
                this.alert(text, "success", time || 2);
            },
            info: function (text, time) {
                this.alert(text, "info", time || 20);
            },
            error: function (text, time) {
                this.alert(text, "danger", time);
            },
            alert: function (message, type, time) {
                if ($.hasValue(message))
                    $.smkAlert({ text: message.nl2Br(), type: type, time: time || 5 });
            },
            confirm: function (message, success) {
                if (!$.hasValue(message))
                    return;

                $.smkConfirm({
                    text: message.nl2Br(),
                    accept: "Sim",
                    cancel: "Não"
                }, function (e) {
                    if (e) {
                        $rootScope.$applyAsync(success);
                    }
                });
            }
        };
    });
    app.directive("showMessage", function ($messages) {
        return function (scope, element, attrs) {
            var type = "error" in attrs ? "error" : "success";
            var time = "time" in attrs ? attrs.time.toInt() : null;

            angular.element(document).ready(function () {
                $messages[type](element.html(), time);
            });
        };
    });

    app.directive("ngLoading", function ($http) {
        return {
            restrict: "A",
            link: function (scope, element) {

                const html = element.html();

                element.click(() => {
                    element.html(`<span class='spinner-border spinner-border-sm'></span> ${html}`);
                });

                function isLoading() {
                    return $http.pendingRequests.length > 0;
                }

                scope.$watch(isLoading, function (v) {
                    element.prop("disabled", v);
                    if (!v) element.html(html);
                });
            }
        };
    });

    // requests
    var $login;
    app.directive("loading", function ($http, $timeout) {
        return {
            restrict: "E",
            priority: 99,
            link: function (scope, element) {
                var timer;
                var loading = element.find("#loading");
                var screenBlock = element.find("#screenBlock");

                function isLoading() {
                    return $http.pendingRequests.length > 0;
                }

                scope.$watch(isLoading, function (v) {
                    if (v) {
                        $timeout.cancel(timer);

                        if ($http.pendingRequests.any("$.showLoader"))
                            loading.fadeIn("fast");

                        if ($http.pendingRequests.any("$.blockScreen"))
                            screenBlock.fadeIn("fast");
                    } else if (loading.is(":visible") || screenBlock.is(":visible")) {
                        timer = $timeout(function () {
                            loading.fadeOut("slow");
                            screenBlock.fadeOut("fast");
                        }, 100);
                    }
                });
            },
            template:
                `<div id='screenBlock'></div>
                <div id='loading'>
                    <div class="spinner-border" style="width: 3rem; height: 3rem;" role="status">
                        <span class="sr-only">Carregando...</span>
                    </div>
                </div>`
        };
    });
    app.service("$login", function () {
        return $login || ($login = {
            dialog: function () {
                throw "$login.dialog(pendingRequest) not implemented";
            }
        });
    });
    app.service("$request", function ($http, $messages, $login) {

        function parseData(data) {
            if (data == null)
                return null;

            if (typeof data == "string") {
                const date = new Regex(/^[0-9:T-]{19}([.0-9]+)?(-[0-9:]{5})$/, data);
                if (date.match)
                    return new Date(data);
            }

            if (typeof data == "object") {
                Object.eachPropertyIn(data, function (key) {
                    data[key] = parseData(data[key]);
                });
            }

            return data;
        }
        function request(options) {
            function makeRequest() {
                $http(options).then(function (r) {
                    validateResult(r.data);
                }, function (r) {
                    validateResult(r.data);
                });
            }
            function validateResult(result) {
                if (!$.isObject(result) || !("Status" in result)) {
                    options.success(parseData(result));
                    return;
                }

                const method = result.Status ? "success" : "error";

                if (result.Authenticated && options[method])
                    options[method](parseData(result.Data));

                if (result.Message) {
                    var time = (result.Status ? 2 : 3) + result.Message.length * 0.03;
                    $messages[method](result.Message, time);
                }

                if (!result.Authenticated)
                    $login.dialog(makeRequest);

                if (result.Data && result.Data.Exception)
                    console.error("$EXCEPTION", result.Data.Exception);
            }

            makeRequest();
        }
        function generateOptions(method, args) {
            var options = args[0];

            if (method === "DELETE" && typeof (options) != "object") {
                options = {};
                options.url = `${args[0]}/${args[1].id}`;
                options.success = args[2];

            } else if (args.length > 1 || typeof (options) != "object") {
                options = {};
                const dataField = method === "GET" ? "params" : "data";

                for (let i in args) {
                    const arg = args[i];
                    if (arg == null) continue;

                    if (typeof arg == "string") {
                        if (!options.url)
                            options.url = arg;
                        else if (!options[dataField])
                            options[dataField] = arg;
                    }

                    if (typeof arg == "object") {
                        if (!options[dataField])
                            options[dataField] = arg;
                    }

                    if (typeof arg == "function") {
                        if (!options.success)
                            options.success = arg;
                        else if (!options.error)
                            options.error = arg;
                    }
                }
            }

            options.method = method;
            options.headers = $.extend(options.headers, { "X-Requested-With": "XMLHttpRequest" });

            if (!("showLoader" in options)) options.showLoader = true;
            if (!("blockScreen" in options)) options.blockScreen = options.method === "POST";

            return options;
        }
        function hasFiles(o) {
            for (var key in o) {
                if (o.hasOwnProperty(key) && !key.startsWith("$$")) {
                    if (o[key] instanceof File)
                        return true;

                    if ($.isObject(o[key]) && hasFiles(o[key]))
                        return true;
                }
            }
            return false;
        }
        function generateFormData(o) {
            var d = new FormData();

            f(o);

            function f(o, k) {
                for (var key in o) {
                    if (o.hasOwnProperty(key) && !key.startsWith("$$")) {
                        if (k != null) {
                            if ($.isArray(o))
                                c(o, k + "[" + key + "]", o[key]);
                            else
                                c(o, k + "." + key, o[key]);
                        } else {
                            c(o, key, o[key]);
                        }
                    }
                }
            }
            function c(o, k, v) {
                if (v == null) return;

                if ($.isObject(v) && !(v instanceof File)) {
                    f(v, k);
                    return;
                }

                if ($.isFunction(v)) {
                    if (v.name !== "value")
                        return;

                    v = v.apply(o);
                }

                if ($.isNumber(v)) {
                    // ReSharper disable once QualifiedExpressionMaybeNull
                    v = v.formatToBr();
                    v = v.replace(/\.|,0+$/g, "");
                }

                d.append(k, v);
            }

            return d;
        }

        return {
            get: function () {
                request(generateOptions("GET", arguments));
            },
            remove: function () {
                request(generateOptions("DELETE", arguments));
            },
            post: function () {
                var options = generateOptions("POST", arguments);

                if (hasFiles(options.data) || options.useFormData) {
                    options.data = generateFormData(options.data);
                    options.headers["Content-Type"] = undefined;
                    options.transformRequest = angular.identity;
                }

                request(options);
            },
            redirect: function (url, params) {
                if (params)
                    url += `?${$.param(params)}`;

                location.href = url;
            }
        }
    });
    app.service("$baseService", function ($request) {
        return function (config) {
            const service = $.extend({}, config);

            if (typeof config.api == "string") {
                config.list = config.get = config.remove = config.save = config.api;
            }
            if (typeof config.all == "string") {
                service.all = function (success) {
                    $request.get(config.all, success);
                };
            }
            if (typeof config.list == "string") {
                service.list = function (filters, success) {
                    $request.get(config.list, filters, function (r) {
                        filters.totalItems = r.Total;
                        success(r.Result);
                    });
                };
            }
            if (typeof config.get == "string") {
                service.get = function (id, success) {
                    $request.get(config.get, { id }, success);
                };
            }
            if (typeof config.remove == "string") {
                service.remove = function (id, success) {
                    $request.remove(config.remove, { id }, success);
                };
            }
            if (typeof config.save == "string") {
                service.save = function (data, success, error) {
                    $request.post(config.save, data, success, error);
                };
            }

            return service;
        };
    });
    app.service("$enumsHelper", function ($request) {
        return function (options) {
            function objectMethod(type) {
                return function (success) {
                    $request.get("/JsonUtil/ListEnums", { typeName: type }, function (enums) {
                        var o = {
                            enums,
                            names: [],
                            filter: function (filters) {
                                return this.enums.where(function (e) {
                                    return !(e.Name in filters) || filters[e.Name];
                                });
                            }
                        };

                        enums.forEach(function (e) {
                            o[e.Name] = e.Value;
                            o.names[e.Value] = e.Display;
                        });

                        success(o);
                    });
                }
            }

            var service = {};
            for (var key in options) {
                service[key] = objectMethod(options[key]);
            }

            return service;
        };
    });

    // format
    app.directive("ngDate", function () {
        return {
            restrict: "A",
            require: "^ngModel",
            scope: {
                maxDate: "=",
                minDate: "="
            },
            link: function (scope, element, attrs, ngModel) {

                function watchDateLimit(property) {
                    scope.$watch(property, function () {
                        var value = scope[property];
                        if (value)
                            value = new DateTime(value).toShortDateString();

                        element.datepicker("option", property, value);
                    });
                }
                function cleanValue(value) {
                    return (value || "").replace(/[^0-9]/g, "");
                }
                function formattedValue(clean) {
                    var value = "";

                    for (var i = 0; i < clean.length && i < 8; i++) {
                        if (i === 2 || i === 4)
                            value += "/";

                        value += clean[i];
                    }

                    return value;
                }
                function convertToStringValue(date) {
                    if (typeof (date) === "string")
                        return date;

                    if (date != null && date != "")
                        return date.toShortDateString();

                    return "";
                }

                ngModel.$formatters.push(convertToStringValue);
                ngModel.$parsers.push(function (value) {
                    if (!value)
                        return null;

                    var clean = cleanValue(value);
                    var formatted = formattedValue(clean);

                    ngModel.$setViewValue(formatted);
                    ngModel.$render();

                    var dateTime = new DateTime(formatted);
                    if (formatted.length === 10 && dateTime.isValid())
                        value = dateTime.value();
                    else
                        value = ngModel.$modelValue;

                    return value;
                });

                watchDateLimit("minDate");
                watchDateLimit("maxDate");

                element.blur(function () {
                    var clean = cleanValue(ngModel.$viewValue);
                    var formatted = formattedValue(clean);

                    var dateTime = new DateTime(formatted);
                    if (formatted.length.between(1, 9) || !dateTime.isValid())
                        formatted = convertToStringValue(ngModel.$modelValue);

                    ngModel.$setViewValue(formatted);
                    ngModel.$render();
                });
                element.datepicker({
                    changeMonth: true,
                    changeYear: true
                });
            }
        };
    });
    app.directive("ngDecimal", function () {
        return {
            restrict: "A",
            require: "^ngModel",
            link: function ($scope, $element, $attrs, $ngModel) {
                var precision = parseInt($attrs.precision || 2);

                var formatter = function (value) {
                    return typeof (value) != "number" || value === 0 ? "" : value.formatToBr(false, precision);
                }
                var parser = function (value) {
                    value = value.toFloat();
                    return isNaN(value) || value === 0 ? null : value;
                }

                if ($.hasValue($attrs.floatValue) && $attrs.floatValue !== "true") {
                    formatter = parser = function (value) {
                        if (typeof (value) == "string") {
                            value = value.toFloat();

                            if (!isNaN(value) && value !== 0)
                                return value.formatToBr();
                        }

                        return "";
                    }
                }

                $ngModel.$formatters.push(formatter);
                $ngModel.$parsers.push(parser);

                $element.maskMoney({
                    prefix: "money" in $attrs ? "R$ " : "",
                    affixesStay: false,
                    decimal: ",",
                    thousands: ".",
                    precision: precision,
                    allowZero: "allowZero" in $attrs,
                    allowNegative: "allowNegative" in $attrs
                });

                if (!$.hasValue($attrs.triggerKey) || $attrs.triggerKey === "true") {
                    $element.keyup(function () { $element.change(); });
                }
            }
        }
    });
    app.directive("ngEnabled", function () {
        return function (scope, element, attrs) {
            scope.$watch(attrs.ngEnabled, function (n) {
                if (!n)
                    element.attr("disabled", "disabled");
                else
                    element.removeAttr("disabled");

                element.toggleClass("disabled", !n);
            });
        }
    });
    app.directive("autoFocusOn", function () {
        return {
            restrict: "A",
            link: function (scope, element, attrs) {
                scope.$watch(attrs.autoFocusOn, function (n, o) {
                    if (n !== o && n === true) {
                        if (element.is(":visible"))
                            element.focus();
                        else
                            window.pendingAutoFocus = element;
                    }
                });
            }
        }
    });

    // validation
    app.directive("validation", function ($messages) {
        return {
            restrict: "E",
            scope: {
                submitEval: "@submit",
                errorPlacement: "&"
            },
            controller: function ($scope) {
                $scope.rules = {};
                $scope.messages = {};

                this.addRule = function (name, rule) {
                    $scope.rules[name] = rule;
                };
                this.addMessages = function (name, messages) {
                    $scope.messages[name] = messages;
                };
            },
            link: function (scope, element, attrs) {
                var form = element.closest("form");
                var settings = {
                    rules: scope.rules,
                    messages: scope.messages
                };

                if ("errorPlacement" in attrs) {
                    settings.errorPlacement = function (error, element) {
                        scope.errorPlacement({
                            event: {
                                message: error.text(),
                                error: error,
                                element: element
                            }
                        });
                        scope.$parent.$apply();
                    };
                }

                form.validate(settings);
                form.submit(function (e) {
                    if (form.valid()) {
                        scope.$parent.$eval(scope.submitEval);
                        scope.$parent.$apply();
                        return true;
                    }

                    e.preventDefault();
                    return false;
                });
            }
        };
    });
    app.directive("validationRule", function () {
        return {
            restrict: "E",
            require: "^validation",
            controller: function ($scope, $element, $attrs) {
                $scope.rule = {};
                $scope.messages = {};

                for (var attr in $attrs) {
                    if (/^(?:(?!\$|name))/.test(attr))
                        $scope.rule[attr] = $attrs[attr];
                }

                this.addMessage = function (type, message) {
                    $scope.messages[type] = message;
                }
            },
            link: function (scope, element, attrs, validation) {
                validation.addRule(attrs.name, scope.rule);
                validation.addMessages(attrs.name, scope.messages);
            }
        }
    });
    app.directive("validationMessages", function () {
        return {
            restrict: "E",
            require: "^validationRule",
            link: function (scope, element, attrs, validationRule) {
                for (var attr in attrs) {
                    if (/^[^\$]/.test(attr))
                        validationRule.addMessage(attr, attrs[attr]);
                }
            }
        }
    });
    app.directive("ngBlurValid", function ($timeout) {
        return {
            restrict: "A",
            required: "^ngModel",
            link: function (scope, element, attrs) {
                var isValid = true;
                var lastValidValue;
                var lastValue;

                scope.$watch(attrs.ngModel, function (v) {
                    lastValue = v;
                });

                element.focus(function () {
                    if (isValid)
                        lastValidValue = lastValue;
                });
                element.blur(function () {
                    // ReSharper disable once CoercedEqualsUsing
                    if (lastValue == lastValidValue) return;

                    isValid = element.valid();

                    if (attrs.ngBlurValid.contains("$event"))
                        execute({ $event: { isValid } });
                    else if (isValid)
                        execute();
                });

                function execute(locals) {
                    $timeout(function () {
                        scope.$eval(attrs.ngBlurValid, locals);
                    });
                }
            }
        }
    });
    app.directive("ngValidateOnFocus", function () {
        return {
            restrict: "A",
            link: function (scope, element) {
                element.focus(function () {
                    element.valid();
                });
            }
        }
    });
    app.directive("requiredField", function ($guid) {
        return {
            strict: "A",
            controller: function ($scope, $attrs) {
                this.isRequired = function () {
                    if (!$.hasValue($attrs.requiredField))
                        return true;

                    return $scope.$eval("!!({0})".format($attrs.requiredField));
                }
            },
            link: function (scope, element, attrs, ctrl) {

                function getValidElements() {
                    var e1 = element.filter("select, input, textarea");
                    var e2 = element.find("select, input, textarea").not("[required-field]");

                    return e1.add(e2);
                }

                var elements = getValidElements();

                scope.$watch(ctrl.isRequired, function (n) {
                    elements.toggleClass("required", n);
                });

                elements.each(function () {
                    if (!$.hasValue($(this).prop("name")))
                        $(this).prop("name", $guid.createElementName());
                });
            }
        }
    });
    app.directive("booleanValidation", function ($guid) {
        return {
            restrict: "E",
            replace: true,
            link: function (scope, element, attrs) {
                var input = element.find("input");

                scope.$watch(attrs.isValid, function (n, o) {
                    if (n === o) return;

                    if (input.attr("aria-invalid") != undefined || input.attr("aria-describedby") != undefined)
                        input.valid();
                });
            },
            template: function (element, attrs) {
                var html =
                    "<div class='form-group'>\
                        <input type='hidden' class='boolean-validation'\
                               name='" + $guid.createElementName() + "'\
                               ng-value=\"!!(" + attrs.isValid + ")\"\
                               message='" + element.html() + "' />\
                    </div>";

                return html;
            }
        }
    });

    // events
    var dragObject;
    app.directive("ngEnterKeypress", function () {
        return {
            restrict: "A",
            link: function ($scope, $element, $attrs) {

                $element.keypress(function (e) {
                    if (e.keyCode === 13) {
                        $scope.$eval($attrs.ngEnterKeypress);
                        e.preventDefault();
                    }
                });
            }
        }
    });
    app.directive("ngPageLoad", function () {
        return {
            restrict: "A",
            link: function (scope, element, attrs) {
                angular.element(document).ready(function () {
                    scope.$eval(attrs.ngPageLoad);
                });
            }
        }
    });
    app.directive("ngDrop", function ($timeout) {
        return {
            restrict: "A",
            link: function (scope, element, attrs) {

                element.on("dragover", function (e) {
                    e.preventDefault();
                });
                element.on("drop", function (e) {
                    e.preventDefault();

                    $timeout(function () {
                        scope.$eval(attrs.ngDrop, {
                            $data: dragObject
                        });
                    });
                });
            }
        }
    });
    app.directive("ngDrag", function () {
        return {
            restrict: "A",
            link: function (scope, element, attrs) {

                element.prop("draggable", true);
                element.on("dragstart", function () {
                    dragObject = scope.$eval(attrs.ngDrag);
                });
            }
        }
    });

    // controls
    function getReplacementAttributes(element) {
        var classes = (element.attr("class") || "").split(" ").where("!$.startsWith('ng-')");
        classes.forEach(function (c) { element.removeClass(c); });

        var name = element.attr("name");
        if (name) element.removeAttr("name");

        return {
            classes: classes.join(" "),
            name
        }
    }
    app.directive("dialog", function ($timeout, $compile) {

        function replaceTag(html, tagName, replacement) {
            var tag = new Regex("<{0}>((.|\\n)*)<\\/{0}>".format(tagName), html);
            if (tag.match)
                html = tag.replace(replacement.replace("{content}", "$1"));

            return html;
        }

        return {
            restrict: "E",
            compile: function (element, attrs) {
                var html = element.html();

                html = replaceTag(html, "dialog-title", "<dialog-header> <h4 class='modal-title'>{content}</h4> </dialog-header>");
                html = replaceTag(html, "dialog-header", "<div class='modal-header'>{content}</div>");
                html = replaceTag(html, "dialog-body", "<div class='modal-body'>{content}</div>");
                html = replaceTag(html, "dialog-footer", "<div class='modal-footer'>{content}</div>");
                html = html.replace(/dialog-form/gi, "form");

                var customAttributes = "static" in attrs ? "data-backdrop='static'" : "tabindex='-1'";
                var replacementAttributes = getReplacementAttributes(element);
                var dialog =
                    "<div class='modal fade' role='dialog'" + customAttributes + " style='display: none'>\
                        <div class='modal-dialog " + replacementAttributes.classes + "' role='document'>\
                            <div class='modal-content'>" + html + "</div>\
                        </div>\
                    </div>";

                element.html("");
                element.removeAttr("class");

                return {
                    post: function (scope, element, attrs) {
                        var target = $("body");
                        var ngView = $("ng-view");

                        if (ngView.length) {
                            target = ngView.find("[ui-yield-to=modals]");
                            if (!target.length) {
                                target = $("<div ui-yield-to='modals'></div>");
                                target.appendTo(ngView);
                            }
                        }

                        dialog = $(dialog);
                        dialog.appendTo(target);
                        $compile(dialog)(scope);

                        scope.$watch(attrs.isOpen, function (newValue, oldValue) {
                            if (newValue) {
                                open();
                                if ("onOpen" in attrs)
                                    scope.$eval(attrs.onOpen);
                            } else {
                                close();
                                if (oldValue && "onClose" in attrs)
                                    scope.$eval(attrs.onClose);
                            }
                        });

                        if ("ngController" in attrs) {
                            var name = attrs.ngController.match(/\w+( as (\w+))?/)[2];
                            if (name && !scope.$parent[name])
                                scope.$parent[name] = scope[name];
                        }

                        dialog.on("show.bs.modal", function () {
                            setTimeout(function () {
                                if (window.pendingAutoFocus) {
                                    window.pendingAutoFocus.focus();
                                    window.pendingAutoFocus = null;
                                }
                            }, 500);
                        });
                        dialog.on("hide.bs.modal", function () {
                            $timeout(function () {
                                scope.$eval(attrs.isOpen + " = false");
                            });

                            dialog.find("form").each(function () {
                                var validator = $(this).data("validator");
                                if (validator)
                                    validator.resetForm();
                            });
                        });

                        function open() {
                            var nextZIndex = null;

                            $(".modal:visible").each(function () {
                                nextZIndex = Math.max(nextZIndex, $(this).css("z-index"));
                            });

                            dialog.modal("show");
                            var backdrop = $(".modal-backdrop").last().insertBefore(dialog);

                            if (nextZIndex != null) {
                                backdrop.css("z-index", nextZIndex + 1);
                                dialog.css("z-index", nextZIndex + 2);
                            }
                        }
                        function close() {
                            dialog.modal("hide");
                            dialog.css("z-index", "");
                        }
                    }
                }
            }
        }
    });
    app.directive("pagination", function () {
        return {
            restrict: "E",
            link: function (scope, element, attrs) {
                var filters = scope.$eval("{0} || ({0} = {})".format(attrs.filters));

                filters.itemsPerPage = parseInt(attrs.itemsPerPage || 10);
                filters.currentPage = parseInt(attrs.currentPage || 1);
            },
            template: function (element, attrs) {

                var hideWhenNoPages = "";
                if ("hideWhenNoPages" in attrs)
                    hideWhenNoPages = " && " + attrs.filters + ".totalItems > " + attrs.filters + ".itemsPerPage'";

                var html =
                    "<ul uib-pagination\
                         items-per-page='" + attrs.filters + ".itemsPerPage'\
                         total-items='" + attrs.filters + ".totalItems'\
                         ng-model='" + attrs.filters + ".currentPage'\
                         ng-change='" + attrs.refresh + "'\
                         ng-show='" + attrs.filters + ".totalItems " + hideWhenNoPages + "'\
                         class='small'\
                         max-size='5'\
                         boundary-links='true'\
                         force-ellipses='true'\
                         previous-text='&lsaquo;'\
                         next-text='&rsaquo;'\
                         first-text='&laquo;'\
                         last-text='&raquo;'>\
                    </ul>";

                return html;
            }
        }
    });
    app.directive("dropdown", function () {

        function getParameters(attrs) {
            var items = attrs.items.match(/^((\w+)?:? in )?([\w.()$'=+ ]+)$/);

            var parameters = {
                item: items[2] || "t",
                items: items[3],
                valueField: attrs.valueField || "Id",
                displayField: attrs.displayField || "Nome",
                groupBy: attrs.groupBy,
                disableWhen: attrs.disableWhen,
                hasFields: !("noFields" in attrs),
                valueAsObject: "objectValue" in attrs && !$.hasValue(attrs.objectValue),
                defaultText: attrs.defaultText || "",
                required: "required" in attrs
            };

            var orderBy = [];

            if ("groupBy" in attrs)
                orderBy.push(attrs.groupBy);

            if ("order" in attrs)
                orderBy.push(parameters.hasFields ? parameters.displayField : "toString()");

            if (orderBy.length)
                parameters.orderBy = orderBy;

            return parameters;
        }
        function getNgOptions(parameters) {
            var value = parameters.item;
            var label = parameters.item;
            var groupBy = "";
            var disableWhen = "";
            var orderBy = "";
            var trackBy = "";

            if (parameters.hasFields && !parameters.valueAsObject)
                value += "." + parameters.valueField;

            if (parameters.hasFields)
                label += "." + parameters.displayField;

            if (parameters.groupBy)
                groupBy = "group by {0}.{1}".format(parameters.item, parameters.groupBy);

            if (parameters.disableWhen)
                disableWhen = "disable when " + parameters.disableWhen;

            if (parameters.orderBy)
                orderBy = "| orderBy: [\"{0}\"]".format(parameters.orderBy.join("\",\""));

            if (parameters.hasFields && parameters.valueAsObject)
                trackBy = "track by " + value;

            return "{0} as {1} {2} {3} for {4} in {5} {6} {7}".format(
                value,
                label,
                groupBy,
                disableWhen,
                parameters.item,
                parameters.items,
                orderBy,
                trackBy
            );
        }

        return {
            restrict: "E",
            require: "^ngModel",
            replace: true,
            link: function (scope, element, attrs) {
                if (!$.hasValue(attrs.objectValue))
                    return;

                var parameters = getParameters(attrs);

                function evaluateObjectValue() {
                    var items = scope.$eval(parameters.items) || [];
                    var value = scope.$eval(attrs.ngModel);

                    // ReSharper disable once UnusedParameter
                    var selected = items.singleOrDefault(null,
                        function (i) {
                            return eval("i." + parameters.valueField) === value;
                        });

                    scope.$eval(attrs.objectValue + " = selected", { selected });
                }

                scope.$watch(attrs.ngModel, evaluateObjectValue);
                scope.$watch(parameters.items, evaluateObjectValue);
                scope.$watch(parameters.items + ".length", evaluateObjectValue);
            },
            template: function (element, attrs) {
                const parameters = getParameters(attrs);
                const ngOptions = getNgOptions(parameters);

                let select = `<select class='form-control' ng-options='${ngOptions}'>`;
                if (!parameters.required)
                    select += `<option value=''>${parameters.defaultText}</option>`;
                select += `</select>`;

                return select;
            }
        };
    });
    app.directive("radioList", function ($guid) {
        return {
            restrict: "E",
            require: ["^ngModel", "?requiredField"],
            scope: {
                items: "=",
                modelValue: "=ngModel"
            },
            link: function (scope, element, attrs, ctrls) {
                scope.isRequired = false;
                scope.change = function () {
                    ctrls[0].$setViewValue(scope.value);
                }

                scope.$watch("modelValue", function (n, o) {
                    if (n === o && n === scope.value) return;

                    scope.value = n;
                });

                if (ctrls[1]) {
                    scope.$watch(ctrls[1].isRequired, function (n, o) {
                        if (n === o && n === scope.isRequired) return;

                        scope.isRequired = n;
                    });
                }
            },
            template: function (element, attrs) {
                var valueField = attrs.valueField || "Id";
                var displayField = attrs.displayField || "Nome";
                var order = "order" in attrs ? "| orderBy: \"" + displayField + "\"" : "";

                var replacementAttrs = getReplacementAttributes(element);
                replacementAttrs.name = replacementAttrs.name || $guid.createElementName();


                var html =
                    "<label class='" + replacementAttrs.classes + " radio-inline' ng-repeat='i in items " + order + "'>\
                        <input type='radio' name='" + replacementAttrs.name + "'\
                                ng-model='$parent.value'\
                                ng-value='i." + valueField + "'\
                                ng-change='change()'\
                                required-field='isRequired' />{{i." + displayField + "}}\
                    </label>";

                return html;
            }
        }
    });

    app.directive("enum", function ($compile, $guid) {

        function getSelectTemplate(element, attrs) {
            const select = $("<select>");

            select.addClass("form-control");
            select.copyAttributesFrom(element, {
                merges: ["class"],
                ignore: ["default-text"],
                replaces: {
                    object: { "ng-options": "e.Value as e.Display for e in {0}.enums".format(attrs.object) }
                }
            });

            select.append("<option value=''>{0}</option>".format(attrs.defaultText || ""));

            return select;
        }
        function getRadioTemplate(element, attrs) {
            const panelAttributes = ["ng-show", "ng-hide", "ng-if", "auto-focus-on"];
            const panel = $("<div>");
            const label = $("<label class='form-check-inline form-check-label'>");
            const radio = $("<input class='form-check-input' />");

            radio.attr("type", "radio");
            radio.copyAttributesFrom(element, {
                ignore: ["class", "default-text", "object", "radio"].union(panelAttributes),
                defaults: {
                    "name": $guid.createElementName()
                }
            });

            label.copyAttributesFrom(element, {
                only: ["class"],
                merges: ["class"]
            });

            if ($.hasValue(attrs.defaultText)) {
                const defaultLabel = $(label.outerHtml());
                const defaultRadio = $(radio.outerHtml());

                defaultRadio.attr("ng-init", "{0} = {0} || null".format(attrs.ngModel));
                defaultRadio.attr("ng-value", "null");
                defaultLabel.append(defaultRadio);
                defaultLabel.append(attrs.defaultText);

                panel.append(defaultLabel);
            }

            radio.attr("ng-value", "e.Value");
            label.copyAttributesFrom(element, {
                only: ["object"],
                replaces: {
                    object: { "ng-repeat": "e in {0}.enums".format(attrs.object) }
                }
            });

            label.append(radio);
            label.append("<span ng-bind='e.Display'></span>");

            panel.append(label);
            panel.copyAttributesFrom(element, {
                only: panelAttributes
            });

            return panel;
        }

        function getCustomRadioTemplate(element, attrs) {
            const panelAttributes = ["ng-show", "ng-hide", "ng-if", "auto-focus-on"];
            const panel = $(`<div>`);
            const div = $(`<div class="custom-control custom-radio custom-control-inline">`);
            const label = $(`<label class="custom-control-label" ng-bind='e.Display'>`);
            const radio = $(`<input class="custom-control-input" />`);
            const elementName = $guid.createElementName();

            radio.attr("type", "radio");
            radio.copyAttributesFrom(element, {
                ignore: ["class", "default-text", "object", "radio"].union(panelAttributes),
                defaults: {
                    "name": elementName
                }
            });

            label.copyAttributesFrom(element, {
                only: ["class"],
                merges: ["class"]
            });

            if ($.hasValue(attrs.defaultText)) {
                const defaultLabel = $(label.outerHtml());
                const defaultRadio = $(radio.outerHtml());

                defaultRadio.attr("ng-init", "{0} = {0} || null".format(attrs.ngModel));
                defaultRadio.attr("ng-value", "null");
                defaultLabel.append(defaultRadio);
                defaultLabel.append(attrs.defaultText);

                panel.append(defaultLabel);
            }

            radio.attr("ng-value", "e.Value");
            radio.attr("id", `${elementName}_{{e.Value}}`);
            label.attr("for", `${elementName}_{{e.Value}}`);
            div.copyAttributesFrom(element, {
                only: ["object"],
                replaces: {
                    object: { "ng-repeat": "e in {0}.enums".format(attrs.object) }
                }
            });

            panel.append(div);
            div.append(radio);
            div.append(label);
            panel.copyAttributesFrom(element, {
                only: panelAttributes
            });

            return panel;
        }

        return {
            directive: "E",
            replace: true,
            link: function (scope, element, attrs) {

                var template = null;
                if ("customRadio" in attrs)
                    template = getCustomRadioTemplate(element, attrs);

                if ("radio" in attrs)
                    template = getRadioTemplate(element, attrs);

                if (!template)
                    template = getSelectTemplate(element, attrs);

                const compiled = $compile(template)(scope);
                element.replaceWith(compiled);
            }
        };
    });
    app.directive("range", function () {
        return {
            restrict: "E",
            require: "^ngModel",
            scope: {
                start: "=",
                end: "=",
                value: "=ngModel"
            },
            replace: true,
            link: function (scope) {

                function generateValues() {
                    var start = scope.start || 0;
                    var end = scope.end;

                    scope.values = Enumerable.Range(start, (end - start) + 1).ToArray();
                }

                scope.$watch("start", generateValues);
                scope.$watch("end", generateValues);
            },
            template:
                "<select class='form-control' ng-options='n for n in values'>\
                    <option value=''></option>\
                </select>"
        };
    });
    app.directive("fileUploader", function (safeApply, $messages) {
        return {
            restrict: "E",
            require: ["^ngModel", "^?requiredField"],
            scope: {
                value: "=ngModel"
            },
            link: function (scope, element, attrs, controls) {
                var ngModel = controls[0];
                var fileElement = element.find(":file");
                var multiple = "multipleFiles" in attrs;
                var extensions = "accept" in attrs ? attrs.accept.split(",").select("$.trim()") : null;

                function getFiles(e) {
                    const files = [];
                    for (let i = 0; i < e.currentTarget.files.length; i++)
                        files.push(e.currentTarget.files[i]);

                    return files;
                }

                function isValid(files) {
                    if (!extensions) return true;

                    return files.all(function (f) {
                        return extensions.any(function (e) {
                            if (f.name.endsWith(e)) return true;

                            if (e.endsWith("/*"))
                                e = e.split("/")[0];

                            return f.type.startsWith(e);
                        });
                    });
                }

                function updateValue(e) {
                    safeApply(scope, function () {
                        const files = getFiles(e);

                        if (isValid(files)) {
                            ngModel.$setViewValue(multiple ? files : files[0]);
                            return;
                        }

                        $messages.error(files.count > 0
                            ? "A extensão de um ou mais arquivos não é válida."
                            : "A extensão do arquivo informado não é valida."
                        );
                        element.val("");
                        ngModel.$setViewValue(null);
                    });
                }

                scope.isRequired = controls[1] ? controls[1].isRequired : null;

                scope.clear = function () {
                    ngModel.$setViewValue(null);
                };

                scope.select = function () {
                    fileElement.trigger("click");
                };

                fileElement.prop("multiple", multiple);
                fileElement.bind("change", updateValue);
                if ("name" in attrs)
                    fileElement.prop("name", attrs.name);

                scope.$watch("value", function (n, o) {
                    safeApply(scope, function () {
                        if (n === o) return;
                        if (n == null) {
                            scope.fileName = null;
                        } else {
                            scope.fileName = !multiple ? scope.value.name : scope.value.select("$.name").join(", ");
                        }
                    });
                });
            },
            template: function (element, attrs) {
                const inputSize = "small" in attrs ? "input-sm" : "";
                const buttonSize = "small" in attrs ? "btn-sm" : "";
                const placeholder = "placeholder" in attrs ? attrs["placeholder"] : "";

                const html = `
                <div class='input-group'>
                    <input type='text' class='form-control ${inputSize}' readonly='readonly' ng-value='fileName' style='max-width: none' required-field='isRequired()||false' placeholder='${placeholder}' />
                    <span class='input-group-append'>
                        <button type='button' class='btn btn-primary ${buttonSize}' ng-show='fileName' ng-click='clear()'>
                            <span class='glyphicon glyphicon-remove'></span> Limpar
                        </button>
                        <button type='button' class='btn btn-primary ${buttonSize}' ng-click='select()'>
                            <span class='glyphicon glyphicon-folder-open'></span>
                            <span style='margin-left: 2px'> {{fileName ? 'Alterar' : 'Procurar'}}</span>
                        </div>
                        <input type='file' style='display: none' />
                    </span>
                </div>`;

                return html;
            }
        };
    });
    app.directive("search", function ($compile) {
        return {
            restrict: "E",
            priority: 999,
            terminal: true,
            compile: function (element, attrs) {
                var addon = $("<addon fa='search'>");
                var bar;
                var barOptions = {
                    merges: ["class"],
                    ignore: [],
                    replaces: {},
                    defaults: {
                        "class": "form-control"
                    }
                };

                if ("ngModel" in attrs || "ngValue" in attrs) {
                    bar = $("<input type='text' />");
                    barOptions.replaces["on-search"] = "ng-enter-keypress";

                    if ("ngModel" in attrs)
                        barOptions.defaults.placeholder = "termo de busca";
                    else
                        barOptions.defaults.readonly = "";
                } else {
                    bar = $("<div>");
                    barOptions.ignore.push("on-search");
                }

                addon.copyAttributesFrom(element, {
                    only: ["class", "on-search", "ng-click"],
                    replaces: {
                        "on-search": "addon-click"
                    }
                });
                bar.copyAttributesFrom(element, barOptions);

                if ("small" in attrs) {
                    addon.addClass("input-sm");
                    bar.addClass("input-sm");
                }

                addon.append(bar);
                element.replaceWith(addon);

                var compiled = $compile(addon);

                return function (scope) {
                    compiled(scope);
                }
            }
        }
    });
    app.directive("wizard", function ($timeout) {
        return {
            restrict: "E",
            scope: true,
            controller: function ($scope, $element, $attrs) {
                var wizard = {};

                this.addStep = function (step) {
                    step.number = $scope.steps.length + 1;
                    step.disabled = true;
                    step.active = false;

                    $scope.steps.push(step);

                    if (step.number === 1)
                        goToStep(step);

                    angular.element(document).ready(function () {
                        var a = $element.find("a[data-toggle='tab'][aria-controls=" + step.number + "]");

                        a.on("show.bs.tab", function (e) {
                            if (!isTabNavigationEnabled()) return false;
                            if ($(e.target).parent().hasClass("disabled")) return false;
                            if (!isStepValid()) return false;

                            goToStep(step);
                            $scope.$apply();

                            return true;
                        });
                    });
                }

                function visibleSteps() {
                    return $scope.steps.where("$.show");
                }
                function getStepByName(name) {
                    return $scope.steps.single("$.name == '{0}'".format(name));
                }

                function getCurrentStepForm() {
                    return $element.find(".tab-pane.active .wizard-form");
                }
                function isTabNavigationEnabled() {
                    var enable = $scope.$eval($attrs.enableTabNavigation);

                    return enable == null || enable;
                }
                function isStepValid() {
                    var form = getCurrentStepForm();
                    return !form.length || form.valid();
                }

                function goToStep(step) {
                    $scope.steps.forEach(function (s) {
                        s.active = false;

                        if (step.number === $scope.steps.length)
                            s.disabled = true;
                    });

                    step.disabled = false;
                    step.active = true;

                    $scope.currentStep = step;
                }
                function incrementStepIndex(increment) {
                    var steps = visibleSteps();
                    var index = steps.indexOf($scope.currentStep);
                    var step = steps[index + increment];

                    if (step) {
                        goToStep(step);
                        return true;
                    }

                    return false;
                }

                function leavingStep(event) {
                    var args = { cancel: false };

                    if (event)
                        event({ wizard, args });

                    return !args.cancel;
                }
                function stepLeft(wizardEvent, stepEvent) {
                    $timeout(function () {
                        if (wizardEvent) $scope.$eval(wizardEvent);
                        if (stepEvent) stepEvent();
                    });
                }

                $scope.steps = [];
                $scope.canGoBack = function () {
                    var steps = visibleSteps();
                    var index = steps.indexOf($scope.currentStep);

                    return index > 0 && !$scope.completed;
                }
                $scope.canGoForward = function () {
                    var steps = visibleSteps();
                    var index = steps.indexOf($scope.currentStep);

                    return index < steps.length && !$scope.completed;
                }
                $scope.goBack = function () {
                    if (leavingStep($scope.currentStep.goingBack)) {
                        var form = getCurrentStepForm();
                        if (form.length)
                            form.validate().resetForm();

                        if (incrementStepIndex(-1))
                            stepLeft($attrs.wentBack, $scope.currentStep.wentBack);
                    }
                }
                $scope.goForward = function () {
                    if (isStepValid() && leavingStep($scope.currentStep.goingForward)) {
                        if (incrementStepIndex(1))
                            stepLeft($attrs.wentForward, $scope.currentStep.wentForward);
                    }
                }

                wizard.goBack = $scope.goBack;
                wizard.goForward = $scope.goForward;
                wizard.goToStep = function (stepName) {
                    goToStep(getStepByName(stepName));
                }
                wizard.complete = function () {
                    $scope.completed = true;
                }

                if ($.hasValue($attrs.name))
                    $scope.$parent[$attrs.name] = wizard;
            },
            compile: function (element) {
                var wizard = $(
                    "<div class='wizard'>\
                        <div class='wizard-inner'>\
                            <div class='connecting-line'></div>\
                            <ul class='nav nav-tabs' role='tablist'>\
                                <li role='presentation'\
                                    ng-repeat='step in steps'\
                                    ng-class='{ active: step.active, disabled: step.disabled }'\
                                    ng-show='step.show'>\
                                    <a data-toggle='tab' aria-controls='{{step.number}}' role='tab' title='{{step.title}}'>\
                                        <span class='round-tab'>\
                                            <i class='fa fa-{{step.icon}}'></i>\
                                        </span>\
                                    </a>\
                                </li>\
                            </ul>\
                        </div>\
                        <div class='tab-content'></div>\
                        <ul class='list-inline pull-right tab-buttons'>\
                            <li ng-show='canGoBack()'>\
                                <button type='button' class='btn btn-secondary prev-step' ng-click='goBack()' ng-bind=\"currentStep.backText || 'Voltar'\"></button>\
                            </li>\
                            <li ng-show='canGoForward()'>\
                                <button type='button' class='btn btn-primary prev-step' ng-click='goForward()' ng-bind=\"currentStep.forwardText || 'Avançar'\"></button>\
                            </li>\
                        </ul>\
                    </div>"
                );

                element.find("step").each(function (i) {
                    var html = $(this).html();
                    var validate = $(this).attr("validate");

                    if (!validate || validate.toBoolean())
                        html = "<form class='wizard-form'> <validation></validation> " + html + " </form>";

                    wizard.find(".tab-content").append(
                        "<div class='tab-pane' role='tabpanel'\
                              ng-class='{ active: steps[" + i + "].active }'>\
                            <div class='tab-pane-content'>" + html + "</div>\
                        </div>"
                    );

                    $(this).html("");
                });

                element.append(wizard);
            }
        }
    });
    app.directive("step", function () {
        return {
            restrict: "E",
            require: "^wizard",
            bindToController: true,
            scope: {
                title: "@",
                icon: "@",
                show: "=?",
                backText: "@",
                forwardText: "@",
                goingBack: "&?",
                goingForward: "&?",
                wentBack: "&?",
                wentForward: "&?"
            },
            link: function (scope, element, attrs, wizard) {
                wizard.addStep(scope.step);
            },
            controllerAs: "step",
            controller: function ($attrs) {
                this.show = true;
                this.name = $attrs.name;
            }
        }
    });
    app.directive("addon", function ($compile) {

        function getAddonIcon(attrs) {
            if ("glyph" in attrs) return { type: "glyphicon", name: attrs.glyph };
            if ("fa" in attrs) return { type: "fa", name: attrs.fa };

            return null;
        }

        return {
            restrict: "E",
            priority: 999,
            terminal: true,
            compile: function (element, attrs) {
                const div = $("<div class='input-group mb-3'>");
                const addon = $("<div>");
                var button;
                const icon = getAddonIcon(attrs);

                if ("addonClick" in attrs) {
                    button = $("<button type='button' class='btn btn-outline-secondary'>");
                    addon.addClass("input-group-append");
                    addon.append(button);
                } else {
                    button = addon;
                    addon.addClass("input-group-addon");
                }

                div.copyAttributesFrom(element, {
                    merges: ["class"],
                    ignore: ["fa", "glyph", "addon-click"]
                });
                button.copyAttributesFrom(element, {
                    only: ["addon-click", "class"],
                    merges: ["class"],
                    replaces: {
                        "addon-click": "ng-click"
                    }
                });

                div.append(element.html());
                div["leftSide" in attrs ? "prepend" : "append"](addon);
                button.append(icon ?
                    "<span class='{0} {0}-{1}'></span>".format(icon.type, icon.name) :
                    attrs.text
                );

                element.replaceWith(div);

                var compiled = $compile(div);

                return function (scope) {
                    compiled(scope);
                }
            }
        }
    });
    app.directive("datepicker", function () {
        return {
            restrict: "E",
            require: "^ngModel",
            scope: {
                maxDate: "=",
                minDate: "="
            },
            replace: true,
            link: function (scope, element) {
                var ngModel = element.find("input").controller("ngModel");

                function cleanValue(value) {
                    return (value || "").replace(/[^0-9]/g, "");
                }
                function formattedValue(clean) {
                    var value = "";

                    for (var i = 0; i < clean.length && i < 8; i++) {
                        if (i === 2 || i === 4)
                            value += "/";

                        value += clean[i];
                    }

                    return value;
                }
                function convertToStringValue(date) {
                    if (date != null)
                        return date.toShortDateString();

                    return null;
                }

                scope.options = {};

                ngModel.$parsers.clear();
                ngModel.$parsers.push(function (value) {
                    if (!value)
                        return null;

                    var clean = cleanValue(value);
                    var formatted = formattedValue(clean);

                    ngModel.$setViewValue(formatted);
                    ngModel.$render();

                    var dateTime = new DateTime(formatted);
                    if (formatted.length === 10 && dateTime.isValid())
                        value = dateTime.value();
                    else
                        value = ngModel.$modelValue;

                    return value;
                });

                scope.onBlur = function () {
                    var clean = cleanValue(ngModel.$viewValue);
                    var formatted = formattedValue(clean);

                    var dateTime = new DateTime(formatted);
                    if (formatted.length.between(1, 9) || !dateTime.isValid())
                        formatted = convertToStringValue(ngModel.$modelValue);

                    ngModel.$setViewValue(formatted);
                    ngModel.$render();
                }

                scope.$watch("minDate", function (v) { scope.options.minDate = v; });
                scope.$watch("maxDate", function (v) { scope.options.maxDate = v; });
            },
            template: function (element, attrs) {
                return "<addon glyph='calendar' addon-click='isOpen = true'>\
                            <input type='text'\
                                   class='form-control'\
                                   uib-datepicker-popup='dd/MM/yyyy'\
                                   ng-model='$parent." + attrs.ngModel + "'\
                                   ng-blur='onBlur()'\
                                   is-open='isOpen'\
                                   datepicker-options='options'>\
                        </addon>";
            }
        }
    });
    app.directive("weekPicker", function () {
        return {
            restrict: "E",
            scope: {
                startDate: "=?",
                endDate: "=?",
                change: "&"
            },
            link: function (scope) {
                scope.options = {
                    showWeeks: false,
                    customClass: getDayClass,
                    initDate: scope.startDate || scope.endDate
                };

                function getDayClass(data) {
                    var date = new DateTime(data.date);

                    if (data.mode === "day" && scope.startDate && scope.endDate) {
                        var diffFromStart = DateTime.dateDiff(scope.startDate, date, true);
                        var diffToEnd = DateTime.dateDiff(scope.endDate, date, true);

                        if (diffFromStart >= 0 && diffToEnd <= 0)
                            return "selected-week";
                    }

                    return "";
                }

                scope.onChange = function () {
                    scope.startDate = new DateTime(scope.selectedDate).startOfWeek().value();
                    scope.endDate = new DateTime(scope.selectedDate).endOfWeek().value();
                }

                scope.$watch("{ startDate, endDate }", scope.change, true);
            },
            template:
                "<div class='input-group'>\
                    <div type='text' class='form-control'\
                           ng-model='selectedDate'\
                           ng-change='onChange()'\
                           uib-datepicker-popup='dd/MM/yyyy'\
                           is-open='isOpen'\
                           datepicker-options='options'\
                           show-button-bar='false'\
                           ng-bind='(startDate | shortDateString) + \" - \" + (endDate | shortDateString)'>\
                    </div>\
                    <span class='input-group-btn'>\
                        <button type='button' class='btn btn-secondary' ng-click='isOpen = true'>\
                            <i class='glyphicon glyphicon-calendar'></i>\
                        </button>\
                    </span>\
                </div>"
        }
    });
    app.directive("draggableList", function () {
        return {
            restrict: "E",
            scope: true,
            link: function (scope, element, attrs) {

                function selectedItem(item) {
                    if (!arguments.length)
                        return scope.$eval(attrs.selectedItem);

                    scope.$parent.$eval(attrs.selectedItem + " = $item", { $item: item });
                    return null;
                }

                scope.onHeaderClicked = function () {
                    var item = selectedItem();

                    if (item) {
                        scope.onDrop(item);
                        selectedItem(null);
                    }
                }
                scope.onDrop = function (item) {
                    scope.$parent.$eval(attrs.onDrop, { $item: item });
                }
                scope.onSelected = function (item) {
                    if (scope.$eval(attrs.selectedItem) === item)
                        selectedItem(null);
                    else
                        selectedItem(item);
                }
            },
            template: function (element, attrs) {
                var panelType = attrs.panelType || "default";
                var panelTitle = attrs.panelTitle;
                var items = attrs.items;
                var selectedItem = attrs.selectedItem;
                var itemText = attrs.itemText;
                var onDrop = attrs.onDrop;
                var acceptDrop = "onDrop" in attrs;

                var html =
                    "<div class='panel panel-" + panelType + "'\
                          " + (onDrop ? "ng-drop='onDrop($data)'" : "") + ">\
                        <div class='panel-heading'\
                             ng-class='{ clickable: " + acceptDrop + " && " + selectedItem + " }'\
                             ng-click='onHeaderClicked()'>" + panelTitle + "</div>\
                        <ul class='list-group list-group-hover list-group-box'>\
                            <li class='list-group-item'\
                                ng-repeat=\"item in " + items + "\"\
                                ng-class='{ active: item == " + selectedItem + " }'\
                                ng-bind='item." + itemText + "'\
                                ng-drag='item'\
                                ng-click='onSelected(item)'>\
                        </ul>\
                    </div>";

                return html;
            }
        };
    });
    app.directive("datatable", function () {

        var buttonTypes = ["info", "success", "danger", "warning", "primary", "secondary"];

        function getSorterPredicateFromBind(bind, itemName) {
            const regex = new RegExp(itemName + "\\.([.\\w]+)(?:$|(?:\\.\\w+\\()|\\)|\\]|\\s)", "m");
            const match = bind.match(regex);
            return match ? match[1] : null;
        }
        function getSorterPredicate(column, itemName) {
            if ($.hasValue(column.attributes.orderable))
                return column.attributes.orderable;

            if ($.hasValue(column.attributes.get))
                return getSorterPredicateFromBind(column.attributes.get, itemName);

            if ($.hasValue(column.attributes.header))
                return column.attributes.header;

            if (!/</gi.test(column.innerHtml))
                return column.innerText;

            return null;
        }

        function createTableElement(element, attrs) {
            const table = $(
                "<table class='table table-sm table-striped datatable'>\
                    <thead> <tr></tr> </thead>\
                    <tbody> <tr></tr> </tbody>\
                </table>"
            );
            const repeater = table.find("tbody tr");

            table.copyAttributesFrom(element, {
                ignore: ["items", "selected"],
                merges: ["class"]
            });
            repeater.copyAttributesFrom(element, {
                only: ["items", "selected"],
                replaces: {
                    "items": "ng-repeat",
                    "selected": "ng-click"
                }
            });

            if ("selected" in attrs)
                repeater.addClass("clickable");

            return table;
        }
        function createButtonElement(button) {
            const isLink = "is-link" in button.attributes ||
                "href" in button.attributes ||
                "ng-href" in button.attributes ||
                "new" in button.attributes && button.attributes.new.startsWith("/") ||
                "edit" in button.attributes && button.attributes.edit.startsWith("/");

            return $(isLink ? "<a>" : "<button type='button'></button>");
        }

        function getButtonType(button) {
            const defaults = {
                new: "success",
                edit: "info",
                remove: "danger"
            };

            for (let key in defaults) {
                if (defaults.hasOwnProperty(key) && key in button.attributes)
                    return defaults[key];
            }

            return buttonTypes.firstOrDefault("secondary", function (type) {
                return type in button.attributes;
            });
        }
        function getButtonIconAttribute(button) {
            var icon = $(button.element).attributesStartWith("icon-")[0];

            if (!icon) {
                const defaults = {
                    new: "plus",
                    edit: "edit",
                    remove: "times"
                };

                for (let key in defaults) {
                    if (defaults.hasOwnProperty(key) && key in button.attributes)
                        icon = { name: "fa", value: defaults[key] };
                }
            }

            return icon;
        }
        function getButtonDestination(button) {
            return "new" in button.attributes || "header" in button.attributes
                ? { container: "thead", cell: "th" }
                : { container: "tbody", cell: "td" };
        }
        function getButtonClickDirective(attribute) {
            return (attribute || "").startsWith("/") ? "ng-href" : "ng-click";
        }

        return {
            restrict: "E",
            scope: true,
            compile: function (element, attrs) {
                var itemName = attrs.items.match(/^\w+/)[0];
                var table = createTableElement(element, attrs);

                element.find("column").each(function () {
                    const column = $(this).information();
                    const th = $("<th>");
                    const td = $("<td>");
                    const sorterPredicate = getSorterPredicate(column, itemName);

                    th.html(column.attributes.header || column.innerHtml);
                    th.copyAttributesFrom(this,
                        {
                            ingore: ["get", "header"],
                            replaces: {
                                orderable: { "ng-click": "order('{0}')".format(sorterPredicate) },
                                initial: { "ng-page-load": "order('{0}')".format(sorterPredicate) }
                            }
                        });

                    if ("orderable" in column.attributes) {
                        th.addClass("hoverPointer");
                        th.append(
                            `<span class='sortorder'
                                ng-class='{ reverse: ${attrs.filters}.Reverse }'
                                ng-show='${attrs.filters}.Predicate =="${sorterPredicate}"'>
                            </span>`
                        );
                    }

                    if (/</gi.test(column.innerHtml))
                        td.html(column.innerHtml);
                    else
                        td.attr("ng-bind", "{0}.{1}".format(itemName, column.innerText.trim()));

                    td.copyAttributesFrom(this,
                        {
                            ignore: ["initial", "orderable"],
                            replaces: {
                                get: "ng-bind"
                            }
                        });

                    td.addClass("align-middle");

                    table.find("thead tr").append(th);
                    table.find("tbody tr").append(td);
                });

                if (element.find("buttons").length) {
                    table.find("thead tr").append("<th class='datatable-buttons'></th>");
                    table.find("tbody tr").append("<td class='datatable-buttons'></td>");

                    element.find("buttons button").each(function () {
                        const button = $(this).information();
                        const btn = createButtonElement(button);
                        const icon = getButtonIconAttribute(button);
                        const destination = getButtonDestination(button);

                        btn.addClass("btn");
                        btn.addClass(`btn-${getButtonType(button)}`);
                        btn.addClass("btn-sm");

                        btn.copyAttributesFrom(this,
                            {
                                ignore: ["fa", "glyphicon"].union(buttonTypes),
                                replaces: {
                                    new: getButtonClickDirective(button.attributes.new),
                                    edit: getButtonClickDirective(button.attributes.edit),
                                    remove: "ng-click"
                                }
                            });

                        btn.html(button.innerHtml);

                        if (icon)
                            btn.html("<span class='{0} {0}-{1}'></span>".format(icon.name.remove("icon-"), icon.value));

                        table.find("{0} tr {1}:last".format(destination.container, destination.cell))
                            .append(` ${btn.outerHtml()}`);
                    });
                    element.find("buttons *:not(button)").each(function () {
                        table.find("tbody tr td:last").append($(this).outerHtml());
                    });
                }

                element.replaceWith(table);

                return function (scope, element, attrs) {
                    scope.order = function (predicate) {
                        const filters = scope.$eval(attrs.filters);

                        filters.Reverse = filters.Predicate === predicate ? !filters.Reverse : false;
                        filters.Predicate = predicate;

                        scope.$eval(attrs.refresh);
                    };
                };
            }
        };
    });
    app.directive("datagroup", function () {
        return {
            restrict: "E",
            scope: true,
            controllerAs: "datagroup",
            controller: function ($attrs) {
                var infos = [];

                this.autoHide = "autoHide" in $attrs;
                this.add = function (info) {
                    infos.push(info);
                }
                this.isVisible = function () {
                    return infos.any("$.isVisible()");
                }
            },
            compile: function (element, attrs) {
                var datagroup = $("<div ng-show='datagroup.isVisible()'>");

                if ("title" in attrs)
                    datagroup.append("<h3>{0}</h3>".format(attrs.title));

                datagroup.append("<dl class='dl-horizontal'>{0}</dl>".format(element.html()));

                element.replaceWith(datagroup.outerHtml());
            }
        }
    });
    app.directive("datainfo", function () {
        return {
            restrcit: "E",
            require: "^datagroup",
            scope: true,
            link: function (scope, element, attrs, datagroup) {

                scope.autoHide = datagroup.autoHide || "autoHide" in attrs;
                scope.isVisible = function () {
                    return !scope.autoHide || $.hasValue(scope.$eval(attrs.value));
                }

                datagroup.add({
                    isVisible: scope.isVisible
                });
            },
            template: function (element) {
                var info = element.information();
                var dt = $("<dt ng-show='isVisible()'>");
                var dd = $("<dd ng-show='isVisible()'>");
                var suffix = info.attributes.suffix ? (`+'${info.attributes.suffix}'`) : "";

                dt.html(info.attributes.title);

                if ($.hasValue(info.html))
                    dd.html(info.html);
                else
                    dd.attr("ng-bind", info.attributes.value + suffix);

                dt.copyAttributesFrom(element, { ignore: ["title", "value", "suffix"] });
                dd.copyAttributesFrom(element, { ignore: ["title", "value", "suffix"] });

                return dt.outerHtml() + dd.outerHtml();
            }
        };
    });

    // text
    app.directive("pluralize", function ($locale) {

        function getOtherAttribute() {
            return $locale.id === "pt-br" ? "outros" : "other";
        }

        return {
            restrict: "E",
            replace: true,
            template: function (element) {
                var html = $("<ng-pluralize></ng-pluralize>");
                var data = {};

                for (var i = 0; i < element[0].attributes.length; i++) {
                    var attribute = element[0].attributes[i];

                    if (/^when-/.test(attribute.name))
                        data[attribute.name.replace("when-", "")] = attribute.value;
                    else if (attribute.name === "else")
                        data[getOtherAttribute()] = attribute.value;
                }

                html.attr("when", JSON.stringify(data));

                return html[0].outerHTML;
            }
        };
    });

    app.directive("onlyLetters", function () {
        return {
            require: "ngModel",
            link: function (scope, element, attr, ngModelCtrl) {
                ngModelCtrl.$parsers.push(function (text) {
                    const transformedInput = text.replace(/[^a-zA-Z ]/g, "");
                    if (transformedInput !== text) {
                        ngModelCtrl.$setViewValue(transformedInput);
                        ngModelCtrl.$render();
                    }
                    return transformedInput;
                });
            }
        };
    });

    app.filter("shortDateString", function () {
        return function (date) {
            if (date && date instanceof Date) {
                date = date.toLocaleString("pt-BR", { timeZone: "America/Sao_Paulo", dateStyle: "short" });
            }

            return date;
        };
    });

    app.filter("shortDateTimeString", function () {
        return function (date) {
            if (date && date instanceof Date) {
                date = date.toLocaleString("pt-BR", { timeZone: "America/Sao_Paulo", dateStyle: "short", timeStyle: "short" });
            }

            return date;
        };
    });

    app.filter("decimal", function () {
        return function (money) {
            if (money)
                money = money.formatToBr();

            return money;
        };
    });
    app.filter("currency", function () {
        return function (money) {
            if (money)
                money = `R$ ${money.formatToBr()}`;

            return money;
        };
    });
    app.filter("booleanString", function () {
        return function (value) {
            return value ? "Sim" : "Não";
        };
    });
    app.filter("nl2Br", function () {
        return function (value) {
            if (!value) return "";

            return value.replace(/\n/gi, "<br/>");
        };
    });

    app.factory("safeApply", [function ($rootScope) {
        return function ($scope, fn) {
            const phase = $scope.$root.$$phase;
            if (phase == "$apply" || phase == "$digest") {
                if (fn) {
                    $scope.$eval(fn);
                }
            } else {
                if (fn) {
                    $scope.$apply(fn);
                } else {
                    $scope.$apply();
                }
            }
        };
    }]);
})();