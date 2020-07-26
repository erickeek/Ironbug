function DateTime() {
    var formatting = "DD/MM/YYYY";
    var m;

    if (arguments.length === 1) {
        var a = arguments[0];

        if (a == null)
            m = moment();
        else if (typeof (a) == "string")
            m = moment(arguments[0], formatting);
        else if (a instanceof Date)
            m = moment(new Date(a));
        else if (a instanceof DateTime)
            m = moment(a.value());
    } else if (arguments.length === 3) {
        if (Array.apply(null, arguments).all("typeof ($) == 'number'"))
            m = moment(new Date(arguments[0], arguments[1], arguments[2]));
    }

    m = m || moment.apply(this, arguments);

    // info
    this.m = function () {
        return m;
    }
    this.value = function () {
        return new Date(m.toDate());
    }
    this.isValid = function () {
        return m.isValid();
    }
    this.format = function (f) {
        return m.format(f || formatting);
    }
    this.clone = function () {
        return new DateTime(m.toDate());
    }

    // gets
    this.date = function () {
        if (arguments.length === 1) {
            m.date(arguments[0].clamp(1, m.daysInMonth()));
            return this;
        }

        return m.date();
    }
    this.month = function () {
        if (arguments.length === 1) {
            m.month(arguments[0].clamp(0, 11));
            return this;
        }

        return m.month();
    }
    this.year = function () {
        if (arguments.length === 1) {
            m.year(arguments[0]);
            return this;
        }

        return m.year();
    }
    this.hour = function () {
        if (arguments.length === 1) {
            m.hour(arguments[0].clamp(0, 23));
            return this;
        }

        return m.hour();
    }
    this.minute = function () {
        if (arguments.length === 1) {
            m.minute(arguments[0].clamp(0, 59));
            return this;
        }

        return m.minute();
    }

    // adds
    this.addMonth = function (value) {
        m.add(value, "M");
        return this;
    }
    this.addDate = function (value) {
        m.add(value, 'd');
        return this;
    }
    this.addYear = function (value) {
        m.add(value, "Y");
        return this;
    }

    // diffs
    this.dateDiff = function (value, ignoreTime) {
        var a = new DateTime(this);
        var b = new DateTime(value);

        if (ignoreTime) {
            a = a.startOfDay();
            b = b.startOfDay();
        }

        return a.m().diff(b.m(), "d");
    }
    this.monthDiff = function (value, ignoreDays) {
        var a = new DateTime(this);
        var b = new DateTime(value);

        if (ignoreDays) {
            a = a.startOfMonth();
            b = b.startOfMonth();
        }

        return a.m().diff(b.m(), "M");
    }
    this.yearDiff = function (value, ignoreMonths) {
        var a = new DateTime(this);
        var b = new DateTime(value);

        if (ignoreMonths) {
            a = a.startOfYear();
            b = b.startOfYear();
        }

        return a.m().diff(b.m(), "Y");
    }

    // equality
    this.equals = function (value, method) {
        value = new DateTime(value);
        var methods = method.split(" ");

        if (methods.contains("date") && this.date() !== value.date()) return false;
        if (methods.contains("month") && this.month() !== value.month()) return false;
        if (methods.contains("year") && this.year() !== value.year()) return false;

        return true;
    }

    // start/end
    this.startOfDay = function () {
        m.startOf("d");
        return this;
    }
    this.startOfWeek = function () {
        m.startOf("week");
        return this;
    }
    this.startOfMonth = function () {
        m.startOf("M");
        return this;
    }
    this.startOfYear = function () {
        m.startOf("Y");
        return this;
    }
    this.endOfWeek = function () {
        m.endOf("week");
        return this;
    }
    this.endOfMonth = function () {
        m.endOf("M");
        return this;
    }
}

DateTime.now = function () {
    return new DateTime();
};
DateTime.today = function () {
    return new DateTime().startOfDay();
};
DateTime.equals = function (d1, d2, method) {
    return new DateTime(d1).equals(d2, method);
};
DateTime.dateDiff = function (initial, final, ignoreTime) {
    return new DateTime(final).dateDiff(initial, ignoreTime);
};
DateTime.monthDiff = function (initial, final, ignoreDays) {
    return new DateTime(final).monthDiff(initial, ignoreDays);
};
DateTime.yearDiff = function (initial, final, ignoreMonths) {
    return new DateTime(final).yearDiff(initial, ignoreMonths);
};
DateTime.prototype.toString = function (f) {
    return this.format(f);
};
Date.toShortDateString = function (v) {
    return v ? v.toShortDateString() : null;
}
Date.prototype.toShortDateString = function () {
    return new DateTime(this).toString();
};
Date.prototype.toDateTime = function () {
    return new DateTime(this);
};
String.prototype.toDateTime = function () {
    return new DateTime(this.toString());
};