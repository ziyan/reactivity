/*
 * Class User
 * Mainly used for current user
 */
var User = function(id, username, name, description, permission) {
    this.id = id;
    this.username = username;
    this.name = name;
    this.description = description;
    this.permission = permission;
};
User.prototype.getId = function() {return this.id;};
User.prototype.getUsername = function() {return this.username;};
User.prototype.getName = function() {return this.name;};
User.prototype.setName = function(value) {this.name = value;};
User.prototype.getDescription = function() {return this.description;};
User.prototype.setDescription = function(value) {this.description = value;};
User.prototype.getPermission = function() {return this.permission;};
User.prototype.setPermission = function(value) {this.permission = value;};
User.prototype.hasAdminPermission = function() {return (this.permission & Util.UserPermission.ADMIN) > 0;};
User.prototype.hasSubscribePermission = function() {return (this.permission & Util.UserPermission.SUBSCRIBE) > 0;};
User.prototype.hasControlPermission = function() {return (this.permission & Util.UserPermission.CONTROL) > 0;};
User.prototype.hasStatsPermission = function() {return (this.permission & Util.UserPermission.STATS) > 0;};

/*
 * Class Building
 */
var Building = function(guid, name, description, longitude, latitude, altitude) {
    this.guid = guid;
    this.name = name;
    this.description = description;
    this.longitude = longitude;
    this.latitude = latitude;
    this.altitude = altitude;
    this.floors = new Array();
};
Building.prototype.getGuid = function() {return this.guid;};
Building.prototype.getName = function() {return this.name;};
Building.prototype.getDescription = function() {return this.description;};
Building.prototype.getLongitude = function() {return this.longitude;};
Building.prototype.getLatitude = function() {return this.latitude;};
Building.prototype.getAltitude = function() {return this.altitude;};
Building.prototype.addFloor = function(floor) {
    if(floor.getBuilding()!=this.getGuid()) return;
    this.floors.push(floor);
};
Building.prototype.getFloor = function(level) {
    for(var i=0;i<this.floors.size();i++)
        if(this.floors[i].getLevel() == level)
            return this.floors[i];
    return null;
};
Building.prototype.getFloors = function() {return this.floors;};

/*
 * Class Floor
 * 
 */
var Floor = function(building, name, description, level, resource) {
    this.building = building;
    this.name = name;
    this.description = description;
    this.level = level;
    this.resource = resource;
};
Floor.prototype.getBuilding = function() {return this.building;};
Floor.prototype.getName = function() {return this.name;};
Floor.prototype.getDescription = function() {return this.description;};
Floor.prototype.getLevel = function() {return this.level;};
Floor.prototype.getResource = function() {return this.resource;};

/*
 * Class Device
 * 
 */
var Device = function(guid, name, description, type, status, building, floor, px, py, pz) {
    this.guid = guid;
    this.name = name;
    this.description = description;
    this.type = type;
    this.status = status;
    this.building = building;
    this.floor = floor;
    this.px = px;
    this.py = py;
    this.pz = pz;
};
Device.prototype.getGuid = function() {return this.guid;};
Device.prototype.getName = function() {return this.name;};
Device.prototype.setName = function(value) {this.name = value;};
Device.prototype.getDescription = function() {return this.description;};
Device.prototype.setDescription = function(value) {this.description = value;};
Device.prototype.getType = function() {return this.type;};
Device.prototype.setType = function(value) {this.type = value;};
Device.prototype.getStatus = function() {return this.status;};
Device.prototype.setStatus = function(value) {this.status = value;};
Device.prototype.getBuilding = function() {return this.building;};
Device.prototype.setBuilding = function(value) {this.building = value;};
Device.prototype.getFloor = function() {return this.floor;};
Device.prototype.setFloor = function(value) {this.floor = value;};
Device.prototype.getPositionX = function() {return this.px;};
Device.prototype.setPositionX = function(value) {this.px = value;};
Device.prototype.getPositionY = function() {return this.py;};
Device.prototype.setPositionY = function(value) {this.py = value;};
Device.prototype.getPositionZ = function() {return this.pz;};
Device.prototype.setPositionZ = function(value) {this.pz = value;};

/*
 * Class Subscription
 * 
 */
var Subscription = function(guid, device, status) {
    this.guid = guid;
    this.device = device;
    this.status = status;
};
Subscription.prototype.getGuid = function() {return this.guid;};
Subscription.prototype.getDevice = function() {return this.device;};
Subscription.prototype.getStatus = function() {return this.status;};
Subscription.prototype.setStatus = function(value) {this.status = value;};


/*
 * Class Statistics
 * 
 */
var Statistics = function(device, service, date, type, count, value) {
    this.device = device;
    this.service = service;
    this.date = date;
    this.type = type;
    this.count = count;
    this.value = value;
};
Statistics.prototype.getDevice = function() {return this.device;};
Statistics.prototype.getService = function() {return this.service;};
Statistics.prototype.getDate = function() {return this.date;};
Statistics.prototype.getType = function() {return this.type;};
Statistics.prototype.getCount = function() {return this.count;};
Statistics.prototype.getValue = function() {return this.value;};


/*
 * Class Reactivity
 * 
 */
var Reactivity = new function() {
function _guid() {return (((1+Math.random())*0x10000)|0).toString(16).substring(1);}
this.session = _guid()+_guid()+"-"+_guid()+"-"+_guid()+"-"+_guid()+"-"+_guid()+_guid()+_guid();
this.server = "http://localhost:88";
this.user = null;
this.devices = null;
this.subscription = null;
this.eventLoopDefaultFreq = 250;
this.eventLoopFreq = this.eventLoopDefaultFreq;
this.eventLoopMinFreq = 3000;
this.eventLoopStop = function() {
	if(this.eventLoopTimer)
		clearTimeout(this.eventLoopTimer);
};
this.eventLoop = function() {
	if(!this.userIsLoggedIn()) return;
	if(this.eventLoopTimer)
		clearTimeout(this.eventLoopTimer);
	var obj = this;
	var req = new XMLHttpRequest();
	req.open("GET", this.server+"/Handler/Event/Get.ashx?session="+this.session, true);
	req.onreadystatechange = function()
	{
		if(req.readyState == 4)
			obj.eventLoopCallback(req.status == 200?eval('('+req.responseText+')'):null);
	};
	req.send();
};
this.eventLoopCallback = function(json) {
	if(!this.userIsLoggedIn()) return;
	if(json) {
		if(json.status!="OK" && this.userIsLoggedIn()) {
			this.userCheckStatus(null);
			if(Gadget) Gadget.refresh();
			return;
		}
		for(var i=0;i<json.events.length;i++)
			this.eventHandler(json.events[i]);
		if(json.events.length>0)
			this.eventLoopFreq = this.eventLoopDefaultFreq;
		else
			this.eventLoopFreq += 250;
		if(this.eventLoopFreq > this.eventLoopMinFreq)
			this.eventLoopFreq = this.eventLoopMinFreq;
	}
	else
		this.eventLoopFreq = this.eventLoopMinFreq;
	if(this.subscription)
		this.eventLoopFreq = this.eventLoopDefaultFreq;
	var obj = this;
	this.eventLoopTimer = setTimeout(function(){obj.eventLoop();}, this.eventLoopFreq);
};
this.eventHandler = function(e) {
	if(e.type=="DeviceCreation" && e.device) {
		if(this.devices==null) return;
		var device = new Device(
			e.device.guid,
			e.device.name,
			e.device.description,
			e.device.type,
			e.device.status,
			e.device.building,
			e.device.floor,
			e.device.x,
			e.device.y,
			e.device.z);
		var exists = false;
		for(var i=0;i<this.devices.length;i++)
			if(this.devices[i].getGuid()==e.device.guid) {
				this.devices[i] = device;
				exists = true;
				break;
			}
		if(!exists) this.devices.push(device);
		if(Gadget) Gadget.update();
	}
	else if(e.type=="DeviceUpdate" && e.device) {
		if(this.devices==null) return;
		for(var i=0;i<this.devices.length;i++)
			if(this.devices[i].getGuid()==e.device.guid) {
				this.devices[i].setName(e.device.name);
				this.devices[i].setDescription(e.device.description);
				this.devices[i].setType(e.device.type);
				this.devices[i].setStatus(e.device.status);
				this.devices[i].setBuilding(e.device.building);
				this.devices[i].setFloor(e.device.floor);
				this.devices[i].setPositionX(e.device.x);
				this.devices[i].setPositionY(e.device.y);
				this.devices[i].setPositionZ(e.device.z);
				break;
			}
		if(Gadget) Gadget.update();
	}
	else if(e.type=="DeviceRemoval" && e.guid) {
		if(this.devices==null) return;
		for(var i=0;i<this.devices.length;i++)
			if(this.devices[i].getGuid()==e.device.guid)
				this.devices.remove(i);
		if(Gadget) Gadget.update();
	}
	else if(e.type=="UserUpdate" && e.user) {
		if(this.user==null) return;
		if(this.user.getId() != e.user.id) return;
		this.user.setName(e.user.name);
		this.user.setDescription(e.user.description);
		this.user.setPermission(e.user.permission);
		if(Gadget) Gadget.update();
	}
	else if(e.type=="UserLogout") {
		this.userCheckStatus(null);
		if(Gadget) Gadget.update();
	}
	else if(e.type=="SubscriptionUpdate" && e.subscription) {
		if(this.subscription==null) return;
		if(this.subscription.getGuid()!=e.subscription.guid) return;
		this.subscription.setStatus(e.subscription.status);
		if(Gadget) Gadget.subscriptionUpdated(this.subscription);
		if(e.subscription.status == "Stopped")
			this.subscription = null;
	}
	else if(e.type=="SubscriptionNotification" && e.data && e.guid) {
		if(this.subscription==null) return;
		if(this.subscription.getGuid()!=e.guid) return;
		var timestamp = new Date();
		timestamp.setTime(parseInt(e.data.timestamp));
		if(Gadget) Gadget.subscriptionNotification(this.subscription, timestamp, e.data.service, e.data.type, e.data.value);
	}
};
this.currentSubscription = function() {
	return this.subscription;
};
this.subscribe = function(device, callback) {
	if(!device||device=="") return false;
	if(!this.userIsLoggedIn()) return false;
	if(!this.userCurrent().hasSubscribePermission()) return false;
	if(this.subscription) return false;
	var obj = this;
	var req = new XMLHttpRequest();
	req.open("GET", this.server+"/Handler/Subscription/Subscribe.ashx?session="+this.session+"&device="+device, true);
	req.onreadystatechange = function() {
		if(req.readyState == 4)
			obj.subscribeCallback(req.status == 200?eval('('+req.responseText+')'):null,callback);
	};
	req.send();
	return true;
};
this.subscribeCallback = function(json,callback) {
	if(!json || json.status!="OK") {
		if(callback) callback(null);
	    return;
	}
	this.subscription = new Subscription(json.subscription.guid,json.subscription.device,json.subscription.status);
	if(callback) callback(this.subscription);
	this.eventLoop();
};

this.unsubscribe = function() {
	if(this.subscription) {
		this.subscription.setStatus("Stopped");
		if(Gadget) Gadget.subscriptionUpdated(this.subscription);
	}
	this.subscription = null;
	var req = new XMLHttpRequest();
	req.open("GET", this.server+"/Handler/Subscription/UnsubscribeAll.ashx?session="+this.session, true);
	req.onreadystatechange = function() {};
	req.send();
};

this.dataSend = function(device, service, type, value, callback) {
	if(!device||device==""||!service||!type||type=="") return false;
	if(!this.userIsLoggedIn()) return false;
	if(!this.userCurrent().hasControlPermission()) return false;
	var obj = this;
	var req = new XMLHttpRequest();
	req.open("GET", this.server+"/Handler/Data/Send.ashx?session="+this.session+"&device="+device+"&service="+service+"&type="+type+"&value="+value, true);
	req.onreadystatechange = function() {
		if(req.readyState == 4)
			obj.dataSendCallback(req.status == 200?eval('('+req.responseText+')'):null,callback);
	};
	req.send();
	return true;
}
this.dataSendCallback = function(json, callback) {
	if (!json) {
		if(callback) callback("ConnectionFailed");
		return;
	}
	if(callback) callback(json.status);
};

this.statisticsQuery = function(device, service, type, start_date, end_date, callback) {
    if(!device || device == "" || !service || !type || !start_date || !end_date || !callback)
        return false;
    if(!this.userIsLoggedIn()) return false;
    if(!this.userCurrent().hasStatsPermission()) return false;
	var obj = this;
	var req = new XMLHttpRequest();
	req.open("GET", this.server+"/Handler/Statistics/Query.ashx?session="+this.session+"&device="+device+"&service="+service+"&type="+type+"&start_date="+start_date.getTime()+"&end_date="+end_date.getTime(), true);
	req.onreadystatechange = function() {
		if(req.readyState == 4)
			obj.statisticsQueryCallback(req.status == 200?eval('('+req.responseText+')'):null,callback);
	};
	req.send();
	return true;
};
this.statisticsQueryCallback = function(json, callback) {
    if (!json) {
        callback(null);
        return;
    }
    if(json.status=="OK" && json.statistics)
    {
        var stats = new Array();
        for(var i=0;i<json.statistics.length;i++)
        {
            var date = new Date();
            date.setTime(parseInt(json.statistics[i].date));
            stats.push(new Statistics(json.statistics[i].device,
                json.statistics[i].service,
                date,
                json.statistics[i].type,
                parseInt(json.statistics[i].count),
                json.statistics[i].value));
        }
        callback(stats);
        return;
    }
    callback(null);
};


this.userIsLoggedIn = function() {
	if(this.user)
		return true;
	return false;
};
this.userCurrent = function() {
	return this.user;
};
this.userLogin = function(uri,username,password,callback) {
	if(!uri||!username||!password) return false;
	var obj = this;
	var req = new XMLHttpRequest();
	req.open("POST", this.server+"/Handler/User/Login.ashx?session="+this.session, true);
	req.setRequestHeader("Content-Type","application/x-www-form-urlencoded");
	req.onreadystatechange = function()
	{
		if(req.readyState == 4)
			obj.userCallback((req.status == 200?eval('('+req.responseText+')'):null),callback);
	};
	req.send("uri="+encodeURI(uri)+"&username="+escape(username)+"&password="+password+"&remember=true");
	return true;
};
this.userCheckStatus = function(callback) {
	var obj = this;
	var req = new XMLHttpRequest();
	req.onreadystatechange = function()
	{
		if(req.readyState == 4)
			obj.userCallback ((req.status == 200?eval('('+req.responseText+')'):null),callback);
	};
	req.open("GET", this.server+"/Handler/User/Current.ashx?session="+this.session, true);
	req.send();
};
this.userCallback = function(json,callback) {
	if(!json) {
		if(callback) callback("ConnectionFailed");
		return;
	}
	if(json.user && (json.status=="LoggedIn"||json.status=="AlreadyLoggedIn")) {
		this.user = new User(json.user.id, json.user.username, json.user.name, json.user.description, json.user.permission);
		if(callback) callback("OK");
		return;
	}
	if(callback) callback(json.status);
};
this.userLogout = function() {
	var req = new XMLHttpRequest();
	req.open("GET",this.server+"/Handler/User/Logout.ashx?session="+this.session+"&forget=true", false);
	req.send();
	this.user = null;
};
this.deviceList = function(callback) {
	if(!this.userIsLoggedIn()) return false;
	if(this.devices) {
		if(callback)
			callback(this.devices);
		return true;
	}
	var obj = this;
	var req = new XMLHttpRequest();
	req.open("GET", this.server+"/Handler/Device/List.ashx?session="+this.session, true);
	req.onreadystatechange = function()
	{
		if(req.readyState == 4)
				obj.deviceListCallback(req.status==200?eval('('+req.responseText+')'):null, callback);
	};
	req.send();
	return true;
};
this.deviceListCallback = function(json, callback) {
	if(!json || json.status!="OK") {
		if(callback) callback(this.devices);
		return;
	}
	this.devices = new Array();
	for(var i=0;i<json.devices.length;i++) {
		this.devices.push(new Device(
			json.devices[i].guid,
			json.devices[i].name,
			json.devices[i].description,
			json.devices[i].type,
			json.devices[i].status,
			json.devices[i].building,
			json.devices[i].floor,
			json.devices[i].x,
			json.devices[i].y,
			json.devices[i].z));
	}
	if(callback) callback(this.devices);
};
this.close = function() {
	var req = new XMLHttpRequest();
	req.open("GET", this.server+"/Handler/Session/Close.ashx?session="+this.session, true);
	req.onreadystatechange = function(){};
	req.send();
};
};

