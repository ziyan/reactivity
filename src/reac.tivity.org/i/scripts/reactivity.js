/*
 * Class User
 * Mainly used for current user
 */
var User = Class.create({
    initialize: function($id, $username, $name, $description, $permission) {
        this.id = $id;
        this.username = $username;
        this.name = $name;
        this.description = $description;
        this.permission = $permission;
    },
    getId: function() {
        return this.id;
    },
    getUsername: function() {
        return this.username;
    },
    getName: function() {
        return this.name;
    },
    setName: function($value) {
        this.name = $value;
    },
    getDescription: function() {
        return this.description;
    },
    setDescription: function($value) {
        this.description = $value;
    },
    getPermission: function() {
        return this.permission;
    },
    setPermission: function($value) {
        this.permission = $value;
    },
    hasAdminPermission: function() {
        return (this.permission & Util.UserPermission.ADMIN) > 0;
    },
    hasSubscribePermission: function() {
        return (this.permission & Util.UserPermission.SUBSCRIBE) > 0;
    },
    hasControlPermission: function() {
        return (this.permission & Util.UserPermission.CONTROL) > 0;
    },
    hasStatsPermission: function() {
        return (this.permission & Util.UserPermission.STATS) > 0;
    }
});

/*
 * Class Building
 * 
 */
var Building = Class.create({
    initialize: function($guid, $name, $description, $longitude, $latitude, $altitude) {
        this.guid = $guid;
        this.name = $name;
        this.description = $description;
        this.longitude = $longitude;
        this.latitude = $latitude;
        this.altitude = $altitude;
        this.floors = new Array();
    },
    getGuid: function() {
        return this.guid;
    },
    getName: function() {
        return this.name;
    },
    getDescription: function() {
        return this.description;
    },
    getLongitude: function() {
        return this.longitude;
    },
    getLatitude: function() {
        return this.latitude;
    },
    getAltitude: function() {
        return this.altitude;
    },
    addFloor: function($floor) {
        if($floor.getBuilding()!=this.getGuid()) return;
        this.floors.push($floor);
    },
    getFloor: function($level) {
        for(var $i=0;$i<this.floors.size();$i++)
            if(this.floors[$i].getLevel() == $level)
                return this.floors[$i];
        return null;
    },
    getFloors: function() {
        return this.floors;
    }
});

/*
 * Class Floor
 * 
 */
var Floor = Class.create({
    initialize: function($building, $name, $description, $level, $resource) {
        this.building = $building;
        this.name = $name;
        this.description = $description;
        this.level = $level;
        this.resource = $resource;
    },
    getBuilding: function() {
        return this.building;
    },
    getName: function() {
        return this.name;
    },
    getDescription: function() {
        return this.description;
    },
    getLevel: function() {
        return this.level;
    },
    getResource: function() {
        return this.resource;
    }
});

/*
 * Class Device
 * 
 */
var Device = Class.create({
    initialize: function($guid, $name, $description, $type, $status, $building, $floor, $px, $py, $pz) {
        this.guid = $guid;
        this.name = $name;
        this.description = $description;
        this.type = $type;
        this.status = $status;
        this.building = $building;
        this.floor = $floor;
        this.px = $px;
        this.py = $py;
        this.pz = $pz;
    },
    getGuid: function() {
        return this.guid;
    },
    getName: function() {
        return this.name;
    },
    setName: function($value) {
        this.name = $value;
    },
    getDescription: function() {
        return this.description;
    },
    setDescription: function($value) {
        this.description = $value;
    },
    getType: function() {
        return this.type;
    },
    setType: function($value) {
        this.type = $value;
    },
    getStatus: function() {
        return this.status;
    },
    setStatus: function($value) {
        this.status = $value;
    },
    getBuilding: function() {
        return this.building;
    },
    setBuilding: function($value) {
        this.building = $value;
    },
    getFloor: function() {
        return this.floor;
    },
    setFloor: function($value) {
        this.floor = $value;
    },
    getPositionX: function() {
        return this.px;
    },
    setPositionX: function($value) {
        this.px = $value;
    },
    getPositionY: function() {
        return this.py;
    },
    setPositionY: function($value) {
        this.py = $value;
    },
    getPositionZ: function() {
        return this.pz;
    },
    setPositionZ: function($value) {
        this.pz = $value;
    }
});


/*
 * Class Subscription
 * 
 */
var Subscription = Class.create({
    initialize: function($guid, $device, $status, $updateCallback, $notificationCallback) {
        this.guid = $guid;
        this.device = $device;
        this.status = $status;
        this.updateCallback = $updateCallback;
        this.notificationCallback = $notificationCallback;
    },
    getGuid: function() {
        return this.guid;
    },
    getDevice: function() {
        return this.device;
    },
    getStatus: function() {
        return this.status;
    },
    setStatus: function($value) {
        this.status = $value;
    },
    getUpdateCallback: function() {
        return this.updateCallback;
    },
    getNotificationCallback: function() {
        return this.notificationCallback;
    }
});


/*
 * Class Statistics
 * 
 */
var Statistics = Class.create({
    initialize: function($device, $service, $date, $type, $count, $value) {
        this.device = $device;
        this.service = $service;
        this.date = $date;
        this.type = $type;
        this.count = $count;
        this.value = $value;
    },
    getDevice: function() {
        return this.device;
    },
    getService: function() {
        return this.service;
    },
    getDate: function() {
        return this.date;
    },
    getType: function() {
        return this.type;
    },
    getCount: function() {
        return this.count;
    },
    getValue: function() {
        return this.value;
    }
});


/*
 * Reactivity
 * 
 */
var Reactivity = new (Class.create({
    initialize: function() {
        this.devices = null;
        this.buildings = null;
        this.user = null;
        this.subscription = null;
        this.eventloopdefaultfreq = 250;
        this.eventloopminfreq = 10000;
        this.eventloopfreq = this.eventloopdefaultfreq;
        this.eventTimer = null;
    },
    eventLoop: function() {
        //return;
        if(!this.userIsLoggedIn()) return;
        if(this.eventTimer)
            clearTimeout(this.eventTimer);
        var $obj = this;
        new Ajax.Request('/Handler/Event/Get.ashx', { method:'get',
            onComplete: function($transport) { $obj.eventLoopCallback($transport) }
        });
    },
    eventLoopCallback: function($transport){
        if(!this.userIsLoggedIn()) return;
        if (200 == $transport.status)
        {
            var $json = $transport.responseText.evalJSON();
            if($json.status != "OK" && this.userIsLoggedIn()) {
                this.refresh();
                return;
            }
            for(var $i=0;$i<$json.events.size();$i++)
                this.eventHandler($json.events[$i]);
                
            //determine frequency
            if($json.events.size()>0)
                this.eventloopfreq = this.eventloopdefaultfreq;
            else
                this.eventloopfreq += 250;
            if(this.eventloopfreq > this.eventloopminfreq)
                this.eventloopfreq = this.eventloopminfreq;
        }
        else
            this.eventloopfreq = this.eventloopminfreq;
        if(this.subscription!=null)
            this.eventloopfreq = this.eventloopdefaultfreq;
        var $obj = this;
        this.eventTimer = setTimeout(function(){$obj.eventLoop();}, this.eventloopfreq);
    },
    eventHandler: function($event) {
        if($event.type=="DeviceCreation" && $event.device) {
            if(this.devices==null) return;
            var $device = new Device(
                    $event.device.guid,
                    $event.device.name,
                    $event.device.description,
                    $event.device.type,
                    $event.device.status,
                    $event.device.building,
                    $event.device.floor,
                    $event.device.x,
                    $event.device.y,
                    $event.device.z
                );
            this.devices.set($device.getGuid(), $device);
            Pages.refresh();
        }
        else if($event.type=="DeviceUpdate" && $event.device) {
            if(this.devices==null) return;
            var $device = this.devices.get($event.device.guid);
            $device.setName($event.device.name);
            $device.setDescription($event.device.description);
            $device.setType($event.device.type);
            $device.setStatus($event.device.status);
            $device.setBuilding($event.device.building);
            $device.setFloor($event.device.floor);
            $device.setPositionX($event.device.x);
            $device.setPositionY($event.device.y);
            $device.setPositionZ($event.device.z);
            this.devices.set($event.device.guid, $device);
            Pages.refresh();
        }
        else if($event.type=="DeviceRemoval" && $event.guid) {
            if(this.devices==null) return;
            this.devices.unset($event.guid);
            Pages.refresh();
        }
        else if($event.type=="UserUpdate" && $event.user) {
            if(this.user==null) return;
            if(this.user.getId() != $event.user.id) return;
            this.user.setName($event.user.name);
            this.user.setDescription($event.user.description);
            this.user.setPermission($event.user.permission);
            Pages.refresh();
        }
        else if($event.type=="UserLogout") {
            this.userLogout(false);
            Pages.refresh();
        }
        else if($event.type=="SubscriptionUpdate" && $event.subscription) {
            if(!this.subscription) return;
            if(this.subscription.getGuid()!=$event.subscription.guid) return;
            this.subscription.setStatus($event.subscription.status);
            var $callback = this.subscription.getUpdateCallback();
            if($callback)
                $callback(this.subscription);
            if($event.subscription.status == "Stopped")
                this.subscription = null;
        }
        else if($event.type=="SubscriptionNotification" && $event.data && $event.guid) {
            if(!this.subscription) return;
            if(this.subscription.getGuid()!=$event.guid) return;
            var $callback = this.subscription.getNotificationCallback();
            var $timestamp = new Date();
            $timestamp.setTime(parseInt($event.data.timestamp));
            if($callback)
                $callback(this.subscription, $timestamp, $event.data.service, $event.data.type, $event.data.value);
        }
    },
    subscribe: function($device, $service, $callback, $updateCallback, $notificationCallback) {
        if(!$device||$device==""||!$updateCallback||!$notificationCallback)
            return false;
        if(this.subscription) return false;
        if(!this.userIsLoggedIn()) return false;
        if(!this.userCurrent().hasSubscribePermission()) return false;
        var $obj = this;
        var $parameters;
        if($service)
            $parameters = {device:$device,service:$service};
        else
            $parameters = {device:$device};
        new Ajax.Request('/Handler/Subscription/Subscribe.ashx', { method:'get',
            parameters:$parameters,
            onComplete: function($transport) {$obj.subscribed($transport,$callback,$updateCallback,$notificationCallback);}
        });
        return true;
    },
    subscribed: function($transport, $callback, $updateCallback, $notificationCallback) {
        if ($transport.status != 200) {
            if($callback)
                $callback(null);
            return;
        }
        var $json = $transport.responseText.evalJSON();
        if($json.status!="OK") {
            if($callback)
                $callback(null);
            return;
        }
        this.subscription = new Subscription($json.subscription.guid,$json.subscription.device,$json.subscription.status,$updateCallback,$notificationCallback);
        if($callback)
            $callback(this.subscription);
        this.eventLoop();
    },
    unsubscribe: function() {
        var $old = this.subscription;
        this.subscription = null;
        if($old) {
            $old.setStatus("Stopped");
            var $callback = $old.getUpdateCallback();
            if($callback) $callback($old);
        }
        new Ajax.Request('/Handler/Subscription/UnsubscribeAll.ashx', { method:'get' });
    },
    dataSend: function($device, $service, $type, $value, $callback) {
        if(!$device||$device==""||!$service||!$type||$type==""||(!$value && $value!=0))
            return false;
        if(!this.userIsLoggedIn()) return false;
        if(!this.userCurrent().hasControlPermission()) return false;
        var $obj = this;
        new Ajax.Request('/Handler/Data/Send.ashx', { method:'get',
            parameters:{device:$device,service:$service,type:$type,value:$value},
            onComplete: function($transport) {$obj.dataSent($transport,$callback);}
        });
        return true;
    },
    dataSent: function($transport, $callback) {
        if ($transport.status != 200) {
            if($callback) $callback("ConnectionFailed");
            return;
        }
        var $json = $transport.responseText.evalJSON();
        if($callback)
            $callback($json.status);
    },
    statisticsQuery: function($device, $service, $type, $start_date, $end_date, $callback) {
        if(!$device || $device == "" || !$service || !$type || !$start_date || !$end_date || !$callback)
            return false;
        if(!this.userIsLoggedIn()) return false;
        if(!this.userCurrent().hasStatsPermission()) return false;
        var $obj = this;
        new Ajax.Request('/Handler/Statistics/Query.ashx', { method:'get',
            parameters:{device:$device,service:$service,type:$type,start_date:$start_date.getTime(),end_date:$end_date.getTime()},
            onComplete: function($transport) {$obj.statisticsQueryCallback($transport,$callback);}
        });
        return true;
    },
    statisticsQueryCallback: function($transport, $callback) {
        if ($transport.status != 200) {
            $callback(null);
            return;
        }
        var $json = $transport.responseText.evalJSON();
        if($json && $json.status=="OK" && $json.statistics)
        {
            var $stats = new Array();
            for(var $i=0;$i<$json.statistics.length;$i++)
            {
                var $date = new Date();
                $date.setTime(parseInt($json.statistics[$i].date));
                $stats.push(new Statistics($json.statistics[$i].device,
                    $json.statistics[$i].service,
                    $date,
                    $json.statistics[$i].type,
                    parseInt($json.statistics[$i].count),
                    $json.statistics[$i].value));
            }
            $callback($stats);
            return;
        }
        $callback(null);
    },
    userIsLoggedIn: function() {
        return this.user != null;
    },
    userCurrent: function() {
        return this.user;
    },
    userLogin: function($uri, $username, $password, $remember, $callback) {
        if(this.userIsLoggedIn()) return false;
        if(!$uri||!$username||!$password) return false;
        if($uri==""||$username==""||$password=="") return false;
        var $obj = this;
        new Ajax.Request('/Handler/User/Login.ashx', { method:'post',
            parameters:{uri:$uri, username:$username, password:Util.hash($password), remember:($remember?"true":"false")},
            onComplete: function($transport) { $obj.userLoginCallback($transport, $callback); }
        });
        return true;
    },
    userLoginCallback: function($transport, $callback) {
        if ($transport.status != 200) {
            if($callback) $callback("ConnectionFailed");
            return;
        }
        var $json = $transport.responseText.evalJSON();
        if(($json.status=="LoggedIn" || $json.status=="AlreadyLoggedIn") && $json.user) {
            this.user = new User($json.user.id, $json.user.username, $json.user.name, $json.user.description, $json.user.permission);
            this.eventLoop();
            Pages.refresh();
        }
        if($callback)
            $callback($json.status);
    },
    userChangePassword: function($old_password, $new_password, $callback) {
        if(!this.userIsLoggedIn()) return false;
        if(!$old_password||!$new_password) return false;
        if($old_password==""||$new_password=="") return false;
        var $obj = this;
        new Ajax.Request('/Handler/User/ChangePassword.ashx', { method:'post',
            parameters:{old_password:Util.hash($old_password), new_password:Util.hash($new_password)},
            onComplete: function($transport) { $obj.userChangePasswordCallback($transport, $callback); }
        });
        return true;
    },
    userChangePasswordCallback: function($transport, $callback) {
        if ($transport.status != 200) {
            if($callback) $callback("ConnectionFailed");
            return;
        }
        var $json = $transport.responseText.evalJSON();
        if($callback) $callback($json.status);
    },
    userLogout: function($forget) {
        this.unsubscribe();
        if($forget)
            new Ajax.Request('/Handler/User/Logout.ashx', { method:'get', parameters:{forget:"true"} });
        else
            new Ajax.Request('/Handler/User/Logout.ashx', { method:'get' });
        this.user = null;
        this.devices = null;
        this.buildings = null;
        Pages.refresh();
    },
    buildingsGet: function($callback) {
        if(!this.userIsLoggedIn()) return false;
        if(this.buildings != null)
        {
            if($callback)
                $callback(this.buildings);
            return true;
        }
        var $obj = this;
        new Ajax.Request('/Handler/Resource/GetIndex.ashx', { method:'get',
            onComplete: function($transport) { $obj.buildingsGetCallback($transport, $callback); }
        });
        return true;
    },
    buildingsGetCallback: function($transport, $callback) {
        if ($transport.status != 200) {
            if($callback) $callback(this.buildings);
            return;
        }
        var $json = $transport.responseText.evalJSON();
        if($json.status=="OK" && $json.buildings) {
            this.buildings = new Hash();
            for(var $b=0;$b<$json.buildings.size();$b++) {
                var $building = new Building(
                        $json.buildings[$b].guid,
                        $json.buildings[$b].name,
                        $json.buildings[$b].description,
                        $json.buildings[$b].longitude,
                        $json.buildings[$b].latitude,
                        $json.buildings[$b].altitude
                    );
                for(var $f=0;$f<$json.buildings[$b].floors.size();$f++) {
                    var $floor = new Floor(
                            $json.buildings[$b].guid,
                            $json.buildings[$b].floors[$f].name,
                            $json.buildings[$b].floors[$f].description,
                            $json.buildings[$b].floors[$f].level,
                            $json.buildings[$b].floors[$f].resource
                        );
                    $building.addFloor($floor);
                }
                this.buildings.set($building.getGuid(), $building);
            }
        }
        if($callback)
            $callback(this.buildings);
    },
    devicesList: function($callback) {
        if(!this.userIsLoggedIn()) return false;
        if(this.devices != null)
        {
            if($callback)
                $callback(this.devices);
            return true;
        }
        var $obj = this;
        new Ajax.Request('/Handler/Device/List.ashx', { method:'get',
            onComplete: function($transport) { $obj.devicesListCallback($transport, $callback); }
        });
        return true;
    },
    devicesListCallback: function($transport, $callback) {
        if ($transport.status != 200) {
            if($callback) $callback(this.devices);
            return;
        }
        var $json = $transport.responseText.evalJSON();
        if($json.status=="OK" && $json.devices) {
            this.devices = new Hash();
            for(var $b=0;$b<$json.devices.size();$b++) {
                var $device = new Device(
                        $json.devices[$b].guid,
                        $json.devices[$b].name,
                        $json.devices[$b].description,
                        $json.devices[$b].type,
                        $json.devices[$b].status,
                        $json.devices[$b].building,
                        $json.devices[$b].floor,
                        $json.devices[$b].x,
                        $json.devices[$b].y,
                        $json.devices[$b].z
                    );
                this.devices.set($device.getGuid(), $device);
            }
        }
        if($callback)
            $callback(this.devices);
    },
    load: function() {
        $("splash").show();
        $("page_content").hide();
        $("floorplan").hide();
        this.unsubscribe();
        var $obj = this;
        new Ajax.Request('/Handler/User/Current.ashx', { method:'get',
            onSuccess: function($transport) { $obj.loaded($transport); },
            onFailure: function($transport) { $("splash_message").innerHTML="Failed to connect to server."; }
        });
    },
    loaded: function($transport) {
        $("splash_message").innerHTML="";
        var $json = $transport.responseText.evalJSON();
        if($json.status=="LoggedIn" && $json.user) {
            this.user = new User($json.user.id, $json.user.username, $json.user.name, $json.user.description, $json.user.permission);
            this.eventLoop();
        } else {
            this.user = null;
        }
        $("splash").hide();
        Pages.clear();
        Pages.watch();
        $("page_content").show();
    },
    unload: function() {
        Pages.unload();
        Reactivity.unsubscribe();
    },
    refresh: function() {
        Pages.stop();
        $("page_content").hide();
        $("splash_message").innerHTML="Reconnecting ...";
        $("splash").show();
        this.unsubscribe();
        this.user = null;
        this.devices = null;
        this.buildings = null;
        var $obj = this;
        new Ajax.Request('/Handler/User/Current.ashx', { method:'get',
            onSuccess: function($transport) { $obj.refreshed($transport); },
            onFailure: function($transport) { $("splash_message").innerHTML="Failed to connect to server."; }
        });
    },
    refreshed: function($transport){
        $("splash_message").innerHTML="";
        var $json = $transport.responseText.evalJSON();
        if($json.status=="LoggedIn" && $json.user) {
            this.user = new User($json.user.id, $json.user.username, $json.user.name, $json.user.description, $json.user.permission);
            this.eventLoop();
        } else {
            this.user = null;
        }
        $("splash").hide();
        Pages.watch();
        Pages.refresh();
        $("page_content").show();
    }
}))();



addEventListener("load",  function() {
    Reactivity.load();
}, true);

addEventListener("unload",  function() {
    Reactivity.unload();
}, true);
