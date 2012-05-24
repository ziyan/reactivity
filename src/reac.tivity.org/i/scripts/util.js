var Util = new function() {
    function hash(s) {
        var chrsz   = 8;
        var hexcase = 0;
        function safe_add (x, y) {
            var lsw = (x & 0xFFFF) + (y & 0xFFFF);
            var msw = (x >> 16) + (y >> 16) + (lsw >> 16);
            return (msw << 16) | (lsw & 0xFFFF);
        }
        function S (X, n) { return ( X >>> n ) | (X << (32 - n)); }
        function R (X, n) { return ( X >>> n ); }
        function Ch(x, y, z) { return ((x & y) ^ ((~x) & z)); }
        function Maj(x, y, z) { return ((x & y) ^ (x & z) ^ (y & z)); }
        function Sigma0256(x) { return (S(x, 2) ^ S(x, 13) ^ S(x, 22)); }
        function Sigma1256(x) { return (S(x, 6) ^ S(x, 11) ^ S(x, 25)); }
        function Gamma0256(x) { return (S(x, 7) ^ S(x, 18) ^ R(x, 3)); }
        function Gamma1256(x) { return (S(x, 17) ^ S(x, 19) ^ R(x, 10)); }
        function core_sha256 (m, l) {
            var K = new Array(0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5, 0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174, 0xE49B69C1, 0xEFBE4786, 0xFC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA, 0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x6CA6351, 0x14292967, 0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85, 0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070, 0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3, 0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2);
            var HASH = new Array(0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19);
            var W = new Array(64);
            var a, b, c, d, e, f, g, h, i, j;
            var T1, T2;
            m[l >> 5] |= 0x80 << (24 - l % 32);
            m[((l + 64 >> 9) << 4) + 15] = l;
            for ( var i = 0; i<m.length; i+=16 ) {
                a = HASH[0];
                b = HASH[1];
                c = HASH[2];
                d = HASH[3];
                e = HASH[4];
                f = HASH[5];
                g = HASH[6];
                h = HASH[7];
                for ( var j = 0; j<64; j++) {
                    if (j < 16) W[j] = m[j + i];
                    else W[j] = safe_add(safe_add(safe_add(Gamma1256(W[j - 2]), W[j - 7]), Gamma0256(W[j - 15])), W[j - 16]);
                    T1 = safe_add(safe_add(safe_add(safe_add(h, Sigma1256(e)), Ch(e, f, g)), K[j]), W[j]);
                    T2 = safe_add(Sigma0256(a), Maj(a, b, c));
                    h = g;
                    g = f;
                    f = e;
                    e = safe_add(d, T1);
                    d = c;
                    c = b;
                    b = a;
                    a = safe_add(T1, T2);
                }
                HASH[0] = safe_add(a, HASH[0]);
                HASH[1] = safe_add(b, HASH[1]);
                HASH[2] = safe_add(c, HASH[2]);
                HASH[3] = safe_add(d, HASH[3]);
                HASH[4] = safe_add(e, HASH[4]);
                HASH[5] = safe_add(f, HASH[5]);
                HASH[6] = safe_add(g, HASH[6]);
                HASH[7] = safe_add(h, HASH[7]);
            }
            return HASH;
        }
        function str2binb (str) {
            var bin = Array();
            var mask = (1 << chrsz) - 1;
            for(var i = 0; i < str.length * chrsz; i += chrsz) {
                bin[i>>5] |= (str.charCodeAt(i / chrsz) & mask) << (24 - i%32);
            }
            return bin;
        }
        function Utf8Encode(string) {
            string = string.replace(/\r\n/g,"\n");
            var utftext = "";
            for (var n = 0; n < string.length; n++) {
                var c = string.charCodeAt(n);
                if (c < 128) {
                    utftext += String.fromCharCode(c);
                }
                else if((c > 127) && (c < 2048)) {
                    utftext += String.fromCharCode((c >> 6) | 192);
                    utftext += String.fromCharCode((c & 63) | 128);
                }
                else {
                    utftext += String.fromCharCode((c >> 12) | 224);
                    utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                    utftext += String.fromCharCode((c & 63) | 128);
                }
            }
            return utftext;
        }
        function binb2hex (binarray) {
            var hex_tab = hexcase ? "0123456789ABCDEF" : "0123456789abcdef";
            var str = "";
            for(var i = 0; i < binarray.length * 4; i++) {
                str += hex_tab.charAt((binarray[i>>2] >> ((3 - i%4)*8+4)) & 0xF) +
                hex_tab.charAt((binarray[i>>2] >> ((3 - i%4)*8  )) & 0xF);
            }
            return str;
        }
        s = Utf8Encode(s);
        return binb2hex(core_sha256(str2binb(s), s.length * chrsz));
    }
    this.hash = hash;
    this.UserPermission = new function() {
        this.ADMIN = 1 << 0;
        this.SUBSCRIBE = 1 << 1;
        this.CONTROL = 1 << 2;
        this.STATS = 1 << 3;
    };
    this.StatisticsType = new function() {
        this.Minutely = 1;
        this.Hourly = 2;
        this.Daily = 3;
        this.Monthly = 4;
    };
    this.DataType = new function() {
        this.Int = "Int";
        this.UInt = "UInt";
        this.Double = "Double";
        this.Float = "Float";
        this.Short = "Short";
        this.UShort = "UShort";
        this.Long = "Long";
        this.ULong = "ULong";
        this.Byte = "Byte";
        this.Bytes = "Bytes";
        this.String = "String";
        this.Bool = "Bool";
    };
    this.ServiceType = new function() {
        this.Default = 1 << 0;
        this.ACNode_Power = 1 << 0;
        this.ACNode_Current = 1 << 1;
        this.ACNode_Voltage = 1 << 2;
        this.ACNode_Relay = 1 << 3;
        this.ComputerNode_CPU = 1 << 0;
        this.ComputerNode_Memory = 1 << 1;
        this.AccelerationSensor_X = 1 << 1;
        this.AccelerationSensor_Y = 1 << 2;
        this.AccelerationSensor_Z = 1 << 3;
    };
    this.DeviceType = new function() {
        this.TemperatureSensor = "00000000-0000-0000-0000-000000000001";
        this.LuminositySensor = "00000000-0000-0000-0000-000000000002";
        this.ACNode = "00000000-0000-0000-0000-000000000003";
        this.MotionSensor = "00000000-0000-0000-0000-000000000004";
        this.RFIDReader = "00000000-0000-0000-0000-000000000005";
        this.ComputerNode = "00000000-0000-0000-0000-000000000006";
        this.AccelerationSensor = "00000000-0000-0000-0000-000000000007";
        this.getName = function($type) {
            if($type==this.TemperatureSensor)
                return "Temperature Sensor";
            if($type==this.LuminositySensor)
                return "Luminosity Sensor";
            if($type==this.ACNode)
                return "AC Node";
            if($type==this.MotionSensor)
                return "Motion Sensor";
            if($type==this.RFIDReader)
                return "RFID Reader";
            if($type==this.ComputerNode)
                return "Computer Node";
            if($type==this.AccelerationSensor)
                return "Acceleration Sensor";
            return "Unknown";
        };
        this.getOnlineIcon = function($type) {
            if($type==this.TemperatureSensor)
                return "images/devices/online/TemperatureSensor.png";
            if($type==this.LuminositySensor)
                return "images/devices/online/LuminositySensor.png";
            if($type==this.ACNode)
                return "images/devices/online/ACNode.png";
            if($type==this.MotionSensor)
                return "images/devices/online/MotionSensor.png";
            if($type==this.RFIDReader)
                return "images/devices/online/RFIDReader.png";
            if($type==this.ComputerNode)
                return "images/devices/online/ComputerNode.png";
            if($type==this.AccelerationSensor)
                return "images/devices/online/AccelerationSensor.png";
            return "images/devices/online/Unknown.png";
        };
        this.getOfflineIcon = function($type) {
            if($type==this.TemperatureSensor)
                return "images/devices/offline/TemperatureSensor.png";
            if($type==this.LuminositySensor)
                return "images/devices/offline/LuminositySensor.png";
            if($type==this.ACNode)
                return "images/devices/offline/ACNode.png";
            if($type==this.MotionSensor)
                return "images/devices/offline/MotionSensor.png";
            if($type==this.RFIDReader)
                return "images/devices/offline/RFIDReader.png";
            if($type==this.ComputerNode)
                return "images/devices/offline/ComputerNode.png";
            if($type==this.AccelerationSensor)
                return "images/devices/offline/AccelerationSensor.png";
            return "images/devices/offline/Unknown.png";
        };
    };
};

var Toggle = Class.create({
    initialize: function($div, $state, $callback) {
        this.div = $div;
        this.state = $state;
        this.callback = $callback;
        var $obj = this;
        this.div.addEventListener("click", function(){$obj.click();});
        this.apply();
    },
    apply: function() {
        if(this.state)
            this.div.setAttribute("class", "toggle toggle_on");
        else
            this.div.setAttribute("class", "toggle toggle_off");
    },
    setState: function($state) {
        //var $old_state = this.state;
        this.state = $state;
        this.apply();
        //if(this.state != $old_state && this.callback)
        //    this.callback(this.state);
    },
    getState: function() {
        return this.state;
    },
    click: function() {
        this.state = !this.state;
        this.apply();
        if(this.callback)
            this.callback(this.state);
    }
});

/*
var Loading = Class.create({
    initialize: function($canvas) {
        this.canvas = $canvas;
        this.bars = 12;
        this.width = 2;
        this.height = 5;
        this.inner = 6;
        this.rate = 50;
        this.color = "rgba(50,79,133,";
        this.timer = null;
        this.current = 0;
        this.ctx = this.canvas.getContext("2d");
    },
    begin: function() {
        this.end();
        this.current = 0;
        this.redraw();
    },
    redraw: function() {
        this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
		this.ctx.save();
		this.ctx.translate(this.ctx.canvas.width/2, this.ctx.canvas.height/2);
		for(var i = 0; i<this.bars; i++)
		{
			var bar = (this.current+i) % this.bars;
		    var angle = 2 * bar * Math.PI / this.bars;
		    var x = this.inner * Math.sin(-angle);
		    var y = this.inner * Math.cos(-angle);
	    	this.ctx.save();
			this.ctx.translate(x, y);
			this.ctx.rotate(angle);
			this.ctx.fillStyle = this.color + ((this.bars+1-i)/(this.bars+1))+ ")";
		    this.ctx.fillRect(-this.width/2, 0, this.width, this.height);
			this.ctx.restore();
		}
		this.ctx.restore();
		this.current = (this.current + 1) % this.bars;
		var obj = this;
		this.timer = setTimeout(function(){obj.redraw();}, this.rate);
    },
    end: function() {
        if(this.timer)
            clearTimeout(this.timer);
        if(this.ctx)
            this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
    }
});*/


/*
 * Class DevicesFilter
 */
var DevicesFilter = new (Class.create({
    initialize: function() {
        this.online = true;
    },
    showOnlineOnly: function($online) {
        this.online = $online;
    },
    populate: function($ul, $building, $floor) {
        if(!$ul) return;
        while($ul.childNodes.length>0)
            $ul.removeChild($ul.childNodes[0]);
        var $li = document.createElement("li");
        $li.innerHTML = "Loading ...<img src=\"images/loading.gif\" class=\"loading\"/>";
        $ul.appendChild($li);
        var $obj = this;
        Reactivity.devicesList(function($devices){$obj.callback($devices, $ul, $building, $floor);});
    },
    callback: function($devices, $ul, $building, $floor) {
        while($ul.childNodes.length>0)
            $ul.removeChild($ul.childNodes[0]);
        if(!$devices) {
            var $li = document.createElement("li");
            $li.innerHTML = "Failed to load devices";
            $ul.appendChild($li);
            return;
        }
        var $toggle_li = document.createElement("li");
        $toggle_li.innerHTML = "Online Only";
        $ul.appendChild($toggle_li);
        var $toggle_div = document.createElement("div");
        var $obj = this;
        new Toggle($toggle_div, this.online, function($state) {
            $obj.showOnlineOnly($state);
            $obj.populate($ul,$building,$floor);
        });
        $toggle_li.appendChild($toggle_div);
        
        var $devices_list = $devices.values();
        for(var $i=0;$i<$devices_list.size();$i++) {
            if($building != null)
                if($devices_list[$i].getBuilding() != $building) continue;
            if($floor != null)
                if($devices_list[$i].getFloor() != $floor) continue;
            if(this.online)
                if($devices_list[$i].getStatus() != "Online") continue;
            var $device_li = document.createElement("li");
            if($devices_list[$i].getStatus()=="Online")
                $device_li.innerHTML = "<img src=\""+Util.DeviceType.getOnlineIcon($devices_list[$i].getType())+"\" class=\"icon\"/><span style=\"color:#235090;font-weight:bold;\">"+$devices_list[$i].getName()+"</span>";
            else
                $device_li.innerHTML = "<img src=\""+Util.DeviceType.getOfflineIcon($devices_list[$i].getType())+"\" class=\"icon\"/>"+$devices_list[$i].getName();
            $device_li.setAttribute("class","linked");
            $device_li.setAttribute("guid", $devices_list[$i].getGuid());
            $device_li.addEventListener("click",function(){Pages.gotoPage(DevicePage,[this.getAttribute("guid")]);},false);
            $ul.appendChild($device_li);
        }
        if($ul.childNodes.length <= 1)
        {
            var $li = document.createElement("li");
            $li.innerHTML = "No available device";
            $ul.appendChild($li);
        }
    }
}))();


var LiveChartSource = Class.create({
    initialize: function($timespan, $strokeColor, $lineWidth) {
        this.timespan = $timespan;
        this.delay = 0;
        this.max = 0;
        this.min = 0;
        this.strokeColor = $strokeColor; //"rgb(30,65,115)"
        this.lineWidth = $lineWidth; //2
        this.xs = new Array();
        this.ys = new Array();
    },
    getMax: function() {
        return this.max;
    },
    getMin: function() {
        return this.min;
    },
    getDelay: function() {
        return this.delay;
    },
    getXs: function() {
        return this.xs;
    },
    getYs: function() {
        return this.ys;
    },
    getLineWidth: function() {
        return this.lineWidth;
    },
    getStrokeColor: function() {
        return this.strokeColor;
    },
    // push new data
    push: function($timestamp, $value) {
        if(this.xs.size() > 0 && this.xs[0].getTime() >= $timestamp.getTime())
            return;
        var $now = new Date();
        if(this.xs.size()<=0)
            this.delay = $now.getTime() - $timestamp.getTime();
        else
            this.delay = parseInt(this.delay * 0.8 + ($now.getTime() - $timestamp.getTime()) * 0.2);
        $now.setTime($now.getTime() - this.delay);
        var $new_xs = new Array();
        var $new_ys = new Array();
        this.min = $value;
        this.max = $value;
        for(var $i=0;$i<this.xs.size();$i++)
        {
            if($now.getTime() - this.xs[$i].getTime() > this.timespan) break;
            $new_xs[$i+1] = this.xs[$i];
            $new_ys[$i+1] = this.ys[$i];
            if($new_ys[$i+1] > this.max)
                this.max = $new_ys[$i+1];
            else if($new_ys[$i+1] < this.min)
                this.min = $new_ys[$i+1];
        }
        $new_xs[0] = $timestamp;
        $new_ys[0] = $value;
        this.xs = $new_xs;
        this.ys = $new_ys;
    },
    clear: function() {
        this.delay = 0;
        this.max = 0;
        this.min = 0;
        this.xs = new Array();
        this.ys = new Array();
    }
});


var LiveChart = Class.create({
    initialize: function($canvas,$timespan) {
        this.canvas = $canvas;
        this.timespan = $timespan;
        this.display = 0.9;
        this.ctx = this.canvas.getContext('2d');
        this.displayHeight = this.ctx.canvas.height * this.display;
        this.displayMargin = (this.ctx.canvas.height - this.displayHeight) / 2;
        this.sources = new Array();
    },
    addSource: function($source) {
        this.sources.push($source);
    },
    redraw: function() {
        //clear canvas
	    this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
	    if(this.sources.size()<=0) return;
	    var $max = 0;
	    var $min = 0;
	    var $delay = 0;
	    var $count = 0;
	    //calculate ranges and delays
	    for(var $i=0;$i<this.sources.size();$i++) {
	        if(this.sources[$i].getXs().size()<2) continue;
	        if($count == 0 || this.sources[$i].getMax()>$max)
	            $max = this.sources[$i].getMax();
	        if($count == 0 || this.sources[$i].getMin()<$min)
	            $min = this.sources[$i].getMin();
	        $delay += this.sources[$i].getDelay();
	        $count++;
	    }
	    if($count<=0) return;
	    $delay = $delay / $count;
        var $now = new Date();
        $now.setTime($now.getTime() - $delay);
        var $range = $max - $min;
        if (!$range) $range = 1;
        for(var $j=0;$j<this.sources.size();$j++) {
            if(this.sources[$j].getXs().size()<2) continue;
            
            var $xs = this.sources[$j].getXs();
            var $ys = this.sources[$j].getYs();
            
            this.ctx.setStrokeColor(this.sources[$j].getStrokeColor());
            this.ctx.setLineWidth(this.sources[$j].getLineWidth());
            this.ctx.beginPath();
            
            this.ctx.moveTo(this.ctx.canvas.width,
                this.displayHeight - (($ys[0] - $min) / $range) * this.displayHeight + this.displayMargin);
            var $overflow = false;
            for(var $i=0;$i<$xs.size();$i++) {
                var $x = this.ctx.canvas.width - ((($now.getTime() - $xs[$i].getTime()) / this.timespan) * this.ctx.canvas.width);
                if($x<0)
                    if(!$overflow) {
                        $x = 0;
                        $overflow = true;
                    }
                    else
                        break;
                if($x>this.ctx.canvas.width) $x=this.ctx.canvas.width;
	            this.ctx.lineTo($x,this.displayHeight - (($ys[$i] - $min) / $range) * this.displayHeight + this.displayMargin);
	        }
	        this.ctx.stroke();
	    }
    },
    removeAllSources: function() {
        this.sources = new Array();
    },
    clear: function() {
        for(var $j=0;$j<this.sources.size();$j++)
            this.sources[$j].clear();
	    this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
    }
});

var LiveChartUpdater = new (Class.create({
    initialize: function() {
        this.active = null;
        this.rate = 500;
        this.timer = null;
    },
    setRate: function($rate) {
        this.rate = $rate;
    },
    begin: function($livechart) {
        this.active = $livechart;
        this.update();
    },
    end: function() {
        if(this.timer)
            clearTimeout(this.timer);
        this.timer = null;
        this.active = null;
    },
    update: function() {
        if(this.timer)
            clearTimeout(this.timer);
        if(!this.active) return;
        this.active.redraw();
        var $obj = this;
        this.timer = setTimeout(function(){$obj.update()}, this.rate);
    }
}))();


var StatisticsChartSource = Class.create({
    initialize: function($start, $end, $strokeColor, $lineWidth) {
        this.max = 0;
        this.min = 0;
        this.start = $start;
        this.end = $end;
        this.strokeColor = $strokeColor; //"rgb(30,65,115)"
        this.lineWidth = $lineWidth; //2
        this.stats = new Array();
    },
    getMax: function() {
        return this.max;
    },
    getMin: function() {
        return this.min;
    },
    getStart: function() {
        return this.start;
    },
    getEnd: function() {
        return this.end;
    },
    getStats: function() {
        return this.stats;
    },
    getLineWidth: function() {
        return this.lineWidth;
    },
    getStrokeColor: function() {
        return this.strokeColor;
    },
    push: function($stat) {
        if($stat.getDate().getTime() > this.end.getTime()) return;
        if($stat.getDate().getTime() < this.start.getTime()) return;
        if(this.stats.size()==0) {
            this.min = $stat.getValue();
            this.max = $stat.getValue();
            this.stats.push($stat);
            return;
        }
        if(this.max < $stat.getValue())
	        this.max = $stat.getValue();
        if(this.min > $stat.getValue())
	        this.min = $stat.getValue();
        this.stats.push($stat);
    },
    clear: function() {
        this.min = 0;
        this.max = 0;
        this.stats = new Array();
    }
});

var StatisticsChart = Class.create({
    initialize: function($canvas) {
        this.canvas = $canvas;
        this.display = 0.9;
        this.ctx = this.canvas.getContext('2d');
        this.displayHeight = this.ctx.canvas.height * this.display;
        this.displayMargin = (this.ctx.canvas.height - this.displayHeight) / 2;
        this.sources = new Array();
    },
    addSource: function($source) {
        this.sources.push($source);
    },
    redraw: function() {
        //clear canvas
	    this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
        if(this.sources.length<=0) return;
        var $max = null;
        var $min = null;
        var $start = null;
        var $end = null;
        //calculate ranges
        for(var $i=0;$i<this.sources.length;$i++) {
        if(this.sources[$i].getStats().length<2) continue;
        if($max == null || this.sources[$i].getMax()>$max)
	        $max = this.sources[$i].getMax();
        if($min == null || this.sources[$i].getMin()<$min)
	        $min = this.sources[$i].getMin();
        if($start == null || this.sources[$i].getStart().getTime()<$start.getTime())
	        $start = this.sources[$i].getStart();
        if($end == null || this.sources[$i].getEnd().getTime()>$end.getTime())
	        $end = this.sources[$i].getEnd();
        }
        if(!$start||!$end) return;
        var $range = $max - $min;
        var $timespan = $end.getTime() - $start.getTime();
        if (!$range) $range = 1;
        if (!$timespan) $timespan = 1;
        for(var $j=0;$j<this.sources.size();$j++) {
            if(this.sources[$j].getStats().size()<2) continue;
            var $stats = this.sources[$j].getStats();
            this.ctx.setStrokeColor(this.sources[$j].getStrokeColor());
            this.ctx.setLineWidth(this.sources[$j].getLineWidth());
            this.ctx.beginPath();
            this.ctx.moveTo(this.ctx.canvas.width,
                this.displayHeight - (($stats[$stats.length-1].getValue() - $min) / $range) * this.displayHeight + this.displayMargin);
            var $overflow = false;
            for(var $i=0;$i<$stats.length;$i++) {
                var $x = this.ctx.canvas.width - ((($end.getTime() - $stats[$i].getDate().getTime()) / $timespan) * this.ctx.canvas.width);
                if($x<0)
                    if(!$overflow) {
                        $x = 0;
                        $overflow = true;
                    }
                    else
                        break;
                if($x>this.ctx.canvas.width) $x=this.ctx.canvas.width;
	            this.ctx.lineTo($x,this.displayHeight - (($stats[$i].getValue() - $min) / $range) * this.displayHeight + this.displayMargin);
	        }
	        this.ctx.stroke();
	    }
    },
    clear: function() {
        for(var $j=0;$j<this.sources.size();$j++)
            this.sources[$j].clear();
        this.sources = new Array();
	    this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
    }
});