// NUMBER
Number.prototype.clamp = function (min, max) {
    return Math.max(Math.min(this, max), min);
};
Number.prototype.formatToBr = function (zeroAsEmpty, precision = 2) {
    const value = this;
    if (value == 0 && zeroAsEmpty)
        return "";

    return value.toLocaleString("pt-BR",
        {
            style: "decimal",
            minimumFractionDigits: precision,
            maximumFractionDigits: precision
        });

    /*precision = precision === undefined ? 2 : precision;

    var value = precision ? this.round(precision) : this;
    if (Number.isNaN(value))
        value = 0;

    var parts = value.toString().split(".");
    var left = parts[0];
    var right = parts[1];

    var result = left.toString();

    for (var i = 0; i < Math.floor((result.length - (1 + i)) / 3); i++)
        result = result.substring(0, result.length - (4 * i + 3)) + '.' + result.substring(result.length - (4 * i + 3));

    if (!right && precision)
        right = "";
    if (precision)
        right = right.padRight(precision, "0");

    if (right)
        result += "," + right;

    if (zeroAsEmpty && result.toFloat() === 0)
        result = "";

    return result;*/
};
Number.prototype.round = function (dec) {
    dec = dec || 0;
    return Math.round(this * Math.pow(10, dec)) / Math.pow(10, dec);
};
Number.prototype.between = function (min, max) {
    return this >= min && this <= max;
};
Number.prototype.toInt = function () {
    return parseInt(this);
};
Number.prototype.ceil = function () {
    return Math.ceil(this);
};
Number.prototype.hasFlag = function (flag) {
    return (this & flag) === flag;
};

// STRING
String.prototype.isEmpty = function () {
    return this.trim() === "";
};
String.prototype.toInt = function () {
    return parseInt(this);
};
String.prototype.toFloat = function () {
    if (this.isEmpty())
        return 0;

    var value = this.replace(/[^0-9\.,-]/gi, '')
        .replace(/\./gi, '')
        .replace(/,/gi, '.');

    return parseFloat(value);
};
String.prototype.format = function () {
    var s = this;
    var i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
};
String.prototype.endsWith = function (suffix) {
    return this.substr(this.length - suffix.length) === suffix;
};
String.prototype.startsWith = function (prefix) {
    return this.substr(0, prefix.length) === prefix;
};
String.prototype.toBoolean = function () {
    return Boolean(JSON.parse(this));
};
String.prototype.lowerFirstLetter = function () {
    return this.charAt(0).toLowerCase() + this.slice(1);
};
String.prototype.upperFirstLetter = function () {
    return this.charAt(0).toUpperCase() + this.slice(1);
};
String.prototype.contains = function (value) {
    return this.indexOf(value) >= 0;
}
String.prototype.padLeft = function (length, char) {
    return Array(Math.max(length - this.length + 1, 0)).join(char || '0') + this;
};
String.prototype.padRight = function (length, char) {
    return this + Array(Math.max(length - this.length + 1, 0)).join(char || '0');
};
String.prototype.nl2Br = function () {
    return this.replace(/\n\r?/g, "<br />");
}
String.prototype.remove = function (value) {
    return this.replace(new RegExp(value, "g"), "");
}

// ARRAY
Array.range = function (start, count) {
    return Enumerable.Range(start, count).ToArray();
}
Array.prototype.clear = function () {
    while (this.length)
        this.pop();

    return this;
};
Array.prototype.addRange = function (values) {
    Array.prototype.push.apply(this, values);

    return this;
};
Array.prototype.remove = function (item) {
    return this.removeRange([item]);
};
Array.prototype.removeRange = function (items) {
    var removeCounter = 0;

    for (var i = 0; i < this.length; i++) {
        if (items.contains(this[i])) {
            this.splice(i, 1);
            removeCounter++;
            i--;
        }
    }

    return removeCounter;
};
Array.prototype.contains = function (item) {
    return $.inArray(item, this) > -1;
};
Array.prototype.equals = function (other) {
    return $(this).not(other).length + $(other).not(this).length === 0;
};
Array.prototype.aggregate = function () {
    var string = $.isString(arguments[0]) ? arguments[0] : "";
    var aggregator = $.isFunction(arguments[1]) ? arguments[1] : arguments[0];

    this.forEach(function (item) {
        string += aggregator(string, item);
    });

    return string;
};
Array.prototype.query = function () {
    return Enumerable.From(this);
};
Array.prototype.single = function (predicate) {
    return this.query().Single(predicate);
};
Array.prototype.singleOrDefault = function (defaultValue, predicate) {
    return this.query().SingleOrDefault(defaultValue, predicate);
};
Array.prototype.first = function (predicate) {
    return this.query().First(predicate);
};
Array.prototype.firstOrDefault = function (defaultValue, predicate) {
    return this.query().FirstOrDefault(defaultValue, predicate);
};
Array.prototype.last = function (predicate) {
    return this.query().Last(predicate);
};
Array.prototype.lastOrDefault = function (defaultValue, predicate) {
    return this.query().LastOrDefault(defaultValue, predicate);
};
Array.prototype.any = function (predicate) {
    return this.query().Any(predicate);
};
Array.prototype.all = function (predicate) {
    return this.query().All(predicate);
};
Array.prototype.sum = function (selector) {
    return this.query().Sum(selector);
};
Array.prototype.max = function (selector) {
    return this.query().Max(selector);
};
Array.prototype.min = function (selector) {
    return this.query().Min(selector);
}
Array.prototype.select = function (selector) {
    var args = Array.prototype.slice.call(arguments);

    if (args.length > 1 && args.all("p => $.isString(p)")) {
        selector = args.aggregate(function (string, item) {
            return (string ? ", " : "") + "{0}: $.{0}".format(item.substring(2));
        });
        selector = "{ {0} }".format(selector);
    }

    return this.query().Select(selector).ToArray();
};
Array.prototype.selectMany = function (collectionSelector, resultSelector) {
    return this.query().SelectMany(collectionSelector, resultSelector).ToArray();
};
Array.prototype.where = function (predicate) {
    return this.query().Where(predicate).ToArray();
};
Array.prototype.orderBy = function (keySelector) {
    return this.query().OrderBy(keySelector).ToArray();
};
Array.prototype.orderByDescending = function (keySelector) {
    return this.query().OrderByDescending(keySelector).ToArray();
};
Array.prototype.groupBy = function (keySelector, elementSelector, resultSelector, compareSelector) {
    return this.query().GroupBy(keySelector, elementSelector, resultSelector, compareSelector).ToArray();
};
Array.prototype.union = function (second, compareSelector) {
    return this.query().Union(second, compareSelector).ToArray();
};
Array.prototype.except = function (second, compareSelector) {
    if (!$.isArrayOrInstanceOf(second))
        second = [second];

    return this.query().Except(second, compareSelector).ToArray();
};
Array.prototype.intersect = function (second, compareSelector) {
    return this.query().Intersect(second, compareSelector).ToArray();
};
Array.prototype.distinct = function (compareSelector) {
    return this.query().Distinct(compareSelector).ToArray();
};
Array.prototype.takeWhile = function (predicate) {
    return this.query().TakeWhile(predicate).ToArray();
};
Array.prototype.take = function (count) {
    return this.query().Take(count).ToArray();
};
Array.prototype.skip = function (count) {
    return this.query().Skip(count).ToArray();
};
Array.prototype.removeAll = function (predicate) {
    return this.removeRange(this.where(predicate));
}

// DATE
Date.prototype.copy = function () {
    return new Date(this);
};

// OBJECT
Object.clean = function (o) {
    for (var key in o) {
        if (o.hasOwnProperty(key))
            delete o[key];
    }
};
Object.eachPropertyIn = function (o, f) {
    if ($.isArrayOrInstanceOf(o)) {
        for (var i = 0; i < o.length; i++)
            f(i);
    } else {
        for (var key in o)
            if (o.hasOwnProperty(key)) f(key);
    }
};

// EXTENDED
$(function () {
    jQuery.extend({
        isArrayOrInstanceOf: function (o) {
            return o instanceof Array || $.isArray(o);
        },
        hasValue: function (o) {
            if (o == null) return false;
            if ($.isString(o) && o.isEmpty()) return false;

            return true;
        },
        postData: function (o) {
            var d = {};

            f(o);

            function f(o, k) {
                for (var key in o) {
                    if (o.hasOwnProperty(key)) {
                        if (k != null) {
                            if ($.isNumeric(key) && $.isArrayOrInstanceOf(o))
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
                if (v == null) {
                    d[k] = "";
                    return;
                }

                if ($.isFunction(v)) {
                    // ReSharper disable once QualifiedExpressionMaybeNull
                    if (v.name !== "value")
                        return;

                    v = v.apply(o);
                }

                if (v instanceof Date) {
                    v = v.toDateTime().toString("DD/MM/YYYY HH:mm:ss");
                } else if ($.isObject(v)) {
                    f(v, k);
                    return;
                }

                if ($.isNumber(v)) {
                    // ReSharper disable once QualifiedExpressionMaybeNull
                    v = v.formatToBr();
                    v = v.replace(/\.|,0+$/g, "");
                }

                d[k] = v;
            }

            return d;
        },
        isBoolean: function (o) {
            return typeof o == "boolean";
        },
        isString: function (o) {
            return typeof o == "string";
        },
        isNumber: function (o) {
            return typeof o == "number";
        },
        isObject: function (o) {
            return typeof o == "object";
        },
        clone: function () {
            var args = Array.prototype.slice.call(arguments);
            args.unshift($.isArrayOrInstanceOf(args[0]) ? [] : {});

            return $.extend.apply($, args);
        },
        deepClone: function () {
            var args = Array.prototype.slice.call(arguments);
            args.unshift($.isArrayOrInstanceOf(args[0]) ? [] : {});
            args.unshift(true);

            return $.extend.apply($, args);
        }
    });
    jQuery.fn.extend({
        outerHtml: function () {
            return this[0].outerHTML;
        },
        copyAttributesFrom: function (other, options) {
            var $default = {
                ignore: [],
                replaces: {},
                defaults: {},
                merges: [],
                only: []
            };

            other = $(other)[0];
            options = $.extend($default, options);

            $.each(this, function () {
                var element = $(this);

                for (var key in options.defaults) {
                    if (options.defaults.hasOwnProperty(key))
                        element.attr(key, options.defaults[key]);
                }

                $.each(other.attributes, function () {
                    if (options.ignore.contains(this.name)) return;
                    if (options.only.length && !options.only.contains(this.name)) return;

                    var replace = options.replaces[this.name];
                    var name = replace || this.name;
                    var value = this.value;

                    if ($.isObject(replace)) {
                        var key = Object.getOwnPropertyNames(replace)[0];
                        name = key;
                        value = replace[key];
                    }

                    if (options.merges.contains(name))
                        value = "{0} {1}".format(element.attr(name), value);

                    element.attr(name, value);
                });
            });
        },
        attributesStartWith: function (value) {
            var attributes = [];

            $.each(this, function () {
                var attributesFound = Enumerable.From(this.attributes)
                    .Where("$.name.startsWith('{0}')".format(value))
                    .ToArray();

                attributes.addRange(attributesFound);
            });

            return attributes;
        },
        information: function () {
            var informations = [];

            $.each(this, function () {
                var info = {
                    element: this,
                    attributes: {},
                    innerHtml: this.innerHTML,
                    outerHtml: this.outerHTML,
                    innerText: this.innerText
                };

                $.each(this.attributes, function () {
                    info.attributes[this.name] = this.value;
                });

                informations.push(info);
            });

            if (informations.length === 1)
                informations = informations[0];

            return informations;
        }
    });
});

// VALIDATE
jQuery.validator.addMethod("lengthEquals", function (value, element, param) {
    return this.optional(element) || value.length === parseInt(param);
}, $.validator.format("Por favor, informe {0} caracteres"));
jQuery.validator.addMethod("booleanValidation", function (value, element) {
    return !$(element).parent().is(":visible") || value === "true";
}, function (param, element) {
    return $(element).attr("message");
});
jQuery.validator.classRuleSettings["boolean-validation"] = { booleanValidation: true };
jQuery.validator.addMethod("minDecimal", function (value, element, param) {
    value = value.toFloat();

    return this.optional(element) ||
        value >= param ||
        value === 0 && $(element).is("[allow-zero]");
}, function (param) {
    return "Por favor, informe um valor igual ou maior que " + param.formatToBr();
});

// CLASSES
function State(obj, options) {
    options = $.extend({
        rules: {}
    }, options);

    var state;

    this.obj = obj;
    this.hasChanges = function () {
        return !equals(state, obj, options.rules);
    }
    this.update = function () {
        state = $.deepClone(obj);
        this.state = state;
    };

    this.update();

    // ReSharper disable CoercedEqualsUsing
    function equals(left, right, rules) {
        if (rules.ignore)
            return true;

        // force undefined types
        left = left || undefined;
        right = right || undefined;

        // left side always has value
        if (left == null) {
            var l = left;

            left = right;
            right = l;
        }

        if (left == null && right == null)
            return true;

        if ($.isBoolean(left)) return booleanEquals(left, right, rules);
        if ($.isNumber(left)) return numberEquals(left, right, rules);
        if ($.isFunction(left)) return functionEquals(left, right, rules);
        if ($.isString(left)) return stringEquals(left, right, rules);
        if ($.isArrayOrInstanceOf(left)) return arrayEquals(left, right, rules);
        if ($.isObject(left)) return objectEquals(left, right, rules);

        return left == right;
    }
    function booleanEquals(left, right) {
        return left == (right || false);
    }
    function numberEquals(left, right) {
        return left == (right || 0);
    }
    function functionEquals(left, right) {
        return $.isFunction(right) || right == null;
    }
    function stringEquals(left, right) {
        left = left.trim();
        right = right || "";

        if ($.isString(right))
            right = right.trim();

        return left == right;
    }
    function arrayEquals(left, right, rules) {
        right = right || [];

        if (!$.isArrayOrInstanceOf(right)) return false;
        if (left.length != right.length) return false;

        if (rules.order || rules.orderBy) {
            var selector = "$" + (rules.orderBy ? "." + rules.orderBy : "");

            left = left.orderBy(selector);
            right = right.orderBy(selector);
        }

        for (var i = 0; i < left.length; i++) {
            var leftItem = left[i];
            var rightItem = right[i];
            var itemRules = rules.element || {};

            if (!equals(leftItem, rightItem, itemRules))
                return false;
        }

        return true;
    }
    function objectEquals(left, right, rules) {
        if ($.isArrayOrInstanceOf(right) || !$.isObject(right))
            return false;

        var leftKeys = getObjectKeys(left);
        var rightKeys = getObjectKeys(right);
        var keys = leftKeys.union(rightKeys);

        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            var leftItem = left[key];
            var rightItem = right[key];
            var itemRules = rules[key] || {};

            if (!equals(leftItem, rightItem, itemRules))
                return false;
        }

        return true;
    }
    // ReSharper restore CoercedEqualsUsing

    function getObjectKeys(o) {
        return Object.getOwnPropertyNames(o).where(function (k) {
            return !$.isFunction(o[k]) && !k.startsWith("$");
        });
    }
}
function Properties(properties) {

    this.copy = function (from) {
        return new function () {
            this.to = function (target) {
                properties.forEach(function (p) {
                    target[p] = from[p];
                });

                return target;
            }
        }
    }
    this.clean = function (target) {
        this.copy({}).to(target);
    }
    this.except = function (except) {
        return new Properties(properties.except(except));
    }
}
function Regex(exp, s) {
    if (typeof exp == "string")
        exp = new RegExp(exp);

    this.exp = exp;
    this.match = s.match(exp);
    this.groups = (this.match || []).slice(1);
    this.replace = function (r) {
        return s.replace(exp, r);
    }
}

function setCookie(cname, cvalue, exdays) {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    const expires = `expires=${d.toUTCString()}`;
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    const name = cname + "=";
    const ca = document.cookie.split(";");
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === " ") {
            c = c.substring(1);
        }
        if (c.indexOf(name) === 0) {
            return JSON.parse(c.substring(name.length, c.length));
        }
    }
    return "";
}
function copyToClipboard(element) {
    const $temp = $("<input>");
    $("body").append($temp);
    $temp.val($(element).text()).select();
    document.execCommand("copy");
    $temp.remove();

    $.smkAlert({
        text: "Conteúdo copiado para área de trasnferência",
        type: "info"
    });
}
