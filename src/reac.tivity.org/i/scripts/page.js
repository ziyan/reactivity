/*
 * Class Page
 * Extend this class to make a new page
 */
var Page = Class.create({
    initialize: function($name, $title, $requirement) {
        this.name = $name.toLowerCase();
        this.args = null;
        this.title = $title;
        this.requirement = $requirement;
    },
    getRequirement: function() {
        return this.requirement;
    },
    getName: function() {
        return this.name;
    },
    getArgs: function() {
        return this.args;
    },
    getTitle: function() {
        return this.title;
    },
    activate: function($args) {
        this.args = $args;
    },
    deactivate: function() {
        this.args = null;
    },
    update: function($args) {
        this.args = $args;
    },
    refresh: function() {
    },
    orientationChanged: function() {
        //setTimeout(function(){window.scrollTo(0, 1);},100);
    }
});


/*
 * Class PageManager
 * PageManager is responsible for going to another page
 */
var Pages = new (Class.create({
    initialize: function() {
        this.pages = new Array();
        this.current = null;
        this.home = null;
        this.login = null;
        this.backButtonPage = null;
        this.backButtonArgs = null;
        this.specialButtonCallback = null;
        this.timer = null;
    },
    addPage: function($page) {
        this.pages.push($page);
    },
    setHome: function($page) {
        this.addPage($page);
        this.home = $page;
    },
    setLogin: function($page) {
        this.addPage($page);
        this.login = $page;
    },
    watch: function() {
        if(this.timer)
            clearTimeout(this.timer);
        //detect bookmarked page and back button
        var string = "";
        if(location.href.indexOf("#")>-1)
            string=location.href.split("#")[1];
        if (string=="") {
            this.gotoPage(null, null);
            return;
        }
        var parts = string.split("/");
        var pagename = parts[0];
        var args = new Array();
        for(var i=1;i<parts.size();i++)
            args[i-1] = parts[i];
        this.gotoPage(this.lookupPage(pagename), args);
        var obj = this;
        this.timer = setTimeout(function(){obj.watch();}, 100);
    },
    stop: function() {
        if(this.timer)
            clearTimeout(this.timer);
    },
    lookupPage: function($name) {
        $name = $name.toLowerCase();
        for(var i=0;i<this.pages.size();i++) {
            if(this.pages[i].getName() == $name)
                return this.pages[i];
        }
        return null;
    },
    go: function($page, $args) {
        this.gotoPage(this.lookupPage($page), $args, true);
    },
    gotoPage: function($page, $args) {
        if(!$args) $args = null;
        if($args && $args.size() == 0)
            $args = null;
        if($page==null)
            $page = this.home;
        var $dirty = false;
        if($page.getRequirement() == 1 && !Reactivity.userIsLoggedIn()) {
            $dirty = true;
            $page = this.login;
            $args = null;
        }
        else if($page.getRequirement() == -1 && Reactivity.userIsLoggedIn()) {
            $dirty = true;
            $page = this.home;
            $args = null;
        }
        if(this.current != $page) {
            //change page
            if(this.current) this.current.deactivate();
            this.clear();
            this.current = $page;
            this.updateAddress(this.current.getName(), $args);
            this.current.activate($args);
            this.setTitle(this.current.getTitle());
            window.scrollTo(0, 1);
            setTimeout(function(){window.scrollTo(0, 1);},100);
            return true;
        } else {
            //update page
            if(this.current) {
                var $cargs = this.current.getArgs();
                if($cargs == null && $args != null)
                    $dirty = true;
                else if($cargs != null && $args != null)
                    if($cargs.size() != $args.size())
                        $dirty = true;
                    else
                        for(var $i=0;$i<$cargs.size();$i++)
                            if($cargs[$i]!=$args[$i])
                            {
                                $dirty = true;
                                break;
                            }
                if($dirty)
                {
                    this.updateAddress(this.current.getName(), $args);
                    this.current.update($args);
                    this.setTitle(this.current.getTitle());
                    window.scrollTo(0, 1);
                    setTimeout(function(){window.scrollTo(0, 1);},100);
                    return true;
                }
            }
        }
        return false;
    },
    refresh: function() {
        if(!this.gotoPage(this.current, this.current ? this.current.getArgs() : null) && this.current) {
            this.current.refresh();
            this.setTitle(this.current.getTitle());
        }
    },
    unload: function() {
        if(this.current)
            this.current.deactivate();
    },
    updateAddress: function($name, $args) {
        var $arg = "";
        if($args)
            for(var $i=0;$i<$args.size();$i++)
                $arg += "/"+$args[$i];
        location.href=location.href.split("#")[0]+"#"+$name+$arg;
    },
    getContent: function() {
        return $("page_content");
    },
    getPool: function() {
        return $("page_pool");
    },
    clear: function() {
        $("floorplan").hide();
        $("backButton").hide();
        $("backButton").innerHTML = "Back";
        this.backButtonPage = null;
        this.backButtonArgs = null;
        $("specialButton").hide();
        $("specialButton").innerHTML = "";
        this.specialButtonCallback = null;
        var $content = $("page_content");
        var $pool = $("page_pool");
        for(var $i=0;$i<$content.childNodes.length;$i++)
            $pool.appendChild($content.childNodes[$i]);
    },
    setTitle: function($title) {
        $("page_title").innerHTML = $title;
        document.title = $title;
    },
    setBackButton: function($text, $page, $args) {
        $("backButton").innerHTML = $text;
        $("backButton").show();
        this.backButtonPage = $page;
        this.backButtonArgs = $args;
    },
    backButtonClicked: function() {
        this.gotoPage(this.backButtonPage, this.backButtonArgs);
    },
    setSpecialButton: function($text, $callback) {
        $("specialButton").innerHTML = $text;
        $("specialButton").show();
        this.specialButtonCallback = $callback;
    },
    specialButtonClicked: function() {
        if(this.specialButtonCallback != null)
            this.specialButtonCallback();
    },
    orientationChanged: function() {
        if(this.current)
            this.current.orientationChanged();
    }
}))();



/*
 * Class HomePage
 * This is the home screen, including a logo and some links
 */
var HomePage = new (Class.create(Page, {
    initialize: function($super) {
        $super("home", "Reactivity", 1);
    },
    activate: function($super, $args) {
        $super($args);
        Pages.getContent().appendChild($("page_home"));
        Pages.setSpecialButton("Logout", function(){if(confirm("Are you sure to logout now?")) Reactivity.userLogout(true);});
        $("home_user").innerHTML = Reactivity.userCurrent().getName();
    },
    refresh: function() {
        $("home_user").innerHTML = Reactivity.userCurrent().getName();
    }
}))();
Pages.setHome(HomePage);

/*
 * Class AboutPage
 * This is the home screen, including a logo and some links
 */
var AboutPage = new (Class.create(Page, {
    initialize: function($super) {
        $super("about", "About", 0);
    },
    activate: function($super, $args) {
        $super($args);
        Pages.setBackButton("Home", HomePage, null);
        Pages.getContent().appendChild($("page_about"));
    }
}))();
Pages.addPage(AboutPage);

/*
 * Class LoginPage
 * This is the login page
 */
var LoginPage = new (Class.create(Page, {
    initialize: function($super) {
        $super("login", "Reactivity", -1);
        this.remember = null;
    },
    activate: function($super, $args) {
        if(!this.remember)
            this.remember = new Toggle($("login_remember"),false,null);
        this.remember.setState(false);
        $("login_progress").hide();
        $("login_before").show();
        Pages.getContent().appendChild($("page_login"));
        Pages.setSpecialButton("About", function(){Pages.gotoPage(AboutPage, null);});
        $super($args);
    },
    login: function() {
        if(Reactivity.userIsLoggedIn())
        {
            //User already logged in
            Pages.gotoPage(HomePage, null);
            return;
        }        
        $("login_before").hide();
        $("login_progress").show();
        var $obj = this;
        if(!Reactivity.userLogin($('login_uri').value,$('login_username').value,$('login_password').value,this.remember.getState(), function($status){$obj.loginCallback($status);})) {
            //Information not valid
            $("login_progress").hide();
            $("login_before").show();
            alert("Please fill in all blanks");
        }
    },
    loginCallback: function($status) {
        if($status=="LoggedIn" || $status=="AlreadyLoggedIn")
        {
            $("login_progress").hide();
            $("login_before").hide();
            $('login_password').value = "";
            return;
        }
        $("login_progress").hide();
        $("login_before").show();
        if($status=="InvalidUri") {
            alert("Invalid Service Uri");
            $('login_uri').focus();
        } else if($status=="ConnectionFailed") {
            alert("Connection Failed");
            $('login_uri').focus();
        } else if($status=="InvalidUsername") {
            alert("Invalid Username");
            $('login_username').focus();
        } else if($status=="InvalidPassword") {
            alert("Invalid Password");
            $('login_password').focus();
        } else {
            alert("Encountered a Server Error");
        }
    }
}))();
Pages.setLogin(LoginPage);

/*
 * Class UserPage
 * This is the user info screen
 */
var UserPage = new (Class.create(Page, {
    initialize: function($super) {
        $super("user", "User", 1);
    },
    activate: function($super, $args) {
        Pages.getContent().appendChild($("page_user"));
        Pages.setBackButton("Home", HomePage, null);
        Pages.setSpecialButton("Logout", function(){if(confirm("Are you sure to logout now?")) Reactivity.userLogout(true);});
        $super($args);
        this.refresh();
    },
    refresh: function() {
        $("user_password_progress").hide();
        $("user_password_form").show();
        $("user_id").innerHTML = Reactivity.userCurrent().getId();
        $("user_username").innerHTML = Reactivity.userCurrent().getUsername();
        $("user_name").innerHTML = Reactivity.userCurrent().getName();
        $("user_description").innerHTML = Reactivity.userCurrent().getDescription();
        this.title = Reactivity.userCurrent().getName();
    },
    changePassword: function() {
        var $password_old = $("user_password_old").value;
        var $password_new = $("user_password_new").value;
        var $password_new2 = $("user_password_new2").value;
        if($password_old==""||$password_new==""||$password_new2=="") {
            alert("Password cannot be empty");
            return;
        }
        if($password_new!=$password_new2) {
            alert("Two new passwords do not match");
            return;
        }
        $("user_password_form").hide();
        $("user_password_progress").show();
        var $obj = this;
        if(!Reactivity.userChangePassword($password_old,$password_new,function($status){$obj.changePasswordCallback($status);})) {
            alert("Error");
            $("user_password_progress").hide();
            $("user_password_form").show();
        }
    },
    changePasswordCallback: function($status) {
        $("user_password_progress").hide();
        $("user_password_form").show();
        if($status == "PasswordChanged") {
            $("user_password_old").value = "";
            $("user_password_new").value = "";
            $("user_password_new2").value = "";
            alert("Your password has been changed");
            return;
        }
        if($status == "InvalidPassword") {
            alert("Your old password is invalid");
            return;
        }
        alert("An error has occured: "+$status);
    }
}))();
Pages.addPage(UserPage);

/*
 * Class BuidingsPage
 * This is the user info screen
 */
var BuildingsPage = new (Class.create(Page, {
    initialize: function($super) {
        $super("buildings", "Buildings", 1);
    },
    activate: function($super, $args) {
        Pages.getContent().appendChild($("page_buildings"));
        Pages.setBackButton("Home", HomePage, null);
        $super($args);
        this.refresh();
    },
    refresh: function() {
        $("buildings_info").hide();
        $("buildings_progress").show();
        var $list = $("buildings_list");
        while($list.childNodes.length>0)
            $list.removeChild($list.childNodes[0]);
        var $obj = this;
        Reactivity.buildingsGet(function($buildings){$obj.callback($buildings);});
    },
    callback: function($buildings) {
        var $list = $("buildings_list");
        while($list.childNodes.length>0)
            $list.removeChild($list.childNodes[0]);
        var $bs = $buildings.values();
        for(var $i=0;$i<$bs.size();$i++) {
            var $b = document.createElement("li");
            $b.innerHTML = "<img src=\"images/icons/building.png\" class=\"icon\"/>"+$bs[$i].getName();
            $b.setAttribute("class","linked");
            $b.setAttribute("guid", $bs[$i].getGuid());
            $b.addEventListener("click",function(){Pages.gotoPage(BuildingPage,[this.getAttribute("guid")]);},false);
            $list.appendChild($b);
        }
        DevicesFilter.populate($("buildings_devices"), null, null);
        $("buildings_progress").hide();
        $("buildings_info").show();
    }
}))();
Pages.addPage(BuildingsPage);


/*
 * Class BuildingPage
 * This is the user info screen
 */
var BuildingPage = new (Class.create(Page, {
    initialize: function($super) {
        $super("building", "Building", 1);
        this.building = null;
    },
    activate: function($super, $args) {
        this.title = "Building";
        Pages.getContent().appendChild($("page_building"));
        Pages.setBackButton("Buildings", BuildingsPage, null);
        $super($args);
        this.refresh();
    },
    update: function($super, $args) {
        $super($args);
        this.refresh();
    },
    refresh: function() {
        $("building_info").hide();
        $("building_progress").show();
        this.building = null;
        var $list = $("building_floors");
        while($list.childNodes.length>0)
            $list.removeChild($list.childNodes[0]);
        var $obj = this;
        Reactivity.buildingsGet(function($buildings){$obj.callback($buildings);});
    },
    callback: function($buildings) {
        if(this.getArgs()==null || this.getArgs().size()<1) {
            Pages.gotoPage(BuildingsPage, null);
            return;
        }
        var $building = $buildings.get(this.getArgs()[0]);
        if(!$building) {
            Pages.gotoPage(BuildingsPage, null);
            return;
        }
        this.building = $building;
        $("building_name").innerHTML = $building.getName();
        $("building_description").innerHTML = $building.getDescription();
        var $list = $("building_floors");
        while($list.childNodes.length>0)
            $list.removeChild($list.childNodes[0]);
        for(var $i=0;$i<$building.getFloors().size();$i++) {
            var $b = document.createElement("li");
            $b.innerHTML = "<img src=\"images/icons/floor.png\" class=\"icon\"/>"+$building.getFloors()[$i].getName();
            $b.setAttribute("class","linked");
            $b.setAttribute("guid", $building.getGuid());
            $b.setAttribute("level", $building.getFloors()[$i].getLevel());
            $b.addEventListener("click",function(){Pages.gotoPage(FloorPage,[this.getAttribute("guid"),this.getAttribute("level")]);},false);
            $list.appendChild($b);
        }
        this.title = $building.getName();
        Pages.setTitle(this.getTitle());
        
        DevicesFilter.populate($("building_devices"), $building.getGuid(), null);
        $("building_progress").hide();
        $("building_info").show();
    },
    showOnMap:function() {
        if(!this.building) return;
        window.open("http://maps.google.com/?q="+this.building.getLongitude()+","+this.building.getLatitude());
    }
}))();
Pages.addPage(BuildingPage);

/*
 * Class FloorPage
 * This is the user info screen
 */
var FloorPage = new (Class.create(Page, {
    initialize: function($super) {
        $super("floor", "Floor", 1);
        this.building = null;
        this.floor = null;
    },
    activate: function($super, $args) {
        this.title = "Floor";
        Pages.getContent().appendChild($("page_floor"));
        Pages.setBackButton("Buildings", BuildingsPage, null);
        $super($args);
        this.refresh();
    },
    update: function($super, $args) {
        $super($args);
        this.refresh();
    },
    refresh: function() {
        $("floor_info").hide();
        $("floor_progress").show();
        this.building = null;
        this.floor = null;
        var $obj = this;
        Reactivity.buildingsGet(function($buildings){$obj.callback($buildings);});
    },
    callback: function($buildings) {
        if(this.getArgs()==null || this.getArgs().size()<1) {
            Pages.gotoPage(BuildingsPage, null);
            return;
        }
        if(this.getArgs().size()<2) {
            Pages.gotoPage(BuildingPage, this.getArgs());
            return;
        }
        if(!$buildings)
        {
            Pages.gotoPage(HomePage, null);
            return;
        }
        var $building = $buildings.get(this.getArgs()[0]);
        if(!$building) {
            Pages.gotoPage(BuildingsPage, null);
            return;
        }
        var $level = parseInt(this.getArgs()[1]);
        var $floor = null;        
        for(var $i=0;$i<$building.getFloors().size();$i++) {
            if($building.getFloors()[$i].getLevel() == $level) {
                $floor = $building.getFloors()[$i];
                break;
            }
        }
        if(!$floor) {
            Pages.gotoPage(BuildingPage, [$building.getGuid()]);
            return;
        }
        this.building = $building;
        this.floor = $floor;
        Pages.setBackButton($building.getName(), BuildingPage, [$building.getGuid()]);
        $("floor_name").innerHTML = $floor.getName();
        $("floor_description").innerHTML = $floor.getDescription();
        $("floor_building").innerHTML = $building.getName();
        $("floor_level").innerHTML = $floor.getLevel();
        this.title = $floor.getName();
        Pages.setTitle(this.getTitle());
        DevicesFilter.populate($("floor_devices"), $building.getGuid(), $floor.getLevel());
        $("floor_progress").hide();
        $("floor_info").show();
    },
    gotoFloorplan: function() {
        if(!this.floor) return;
        Pages.gotoPage(FloorplanPage, [this.floor.getBuilding(),this.floor.getLevel()]);
    },
    gotoBuilding: function() {
        if(!this.building) return;
        Pages.gotoPage(BuildingPage, [this.building.getGuid()]);
    }
}))();
Pages.addPage(FloorPage);

/*
 * Class FloorplanPage
 * This is the user info screen
 */
var FloorplanPage = new (Class.create(Page, {
    initialize: function($super) {
        $super("floorplan", "Floor Plan", 1);
        this.building = null;
        this.floor = null;
        this.device = null;
        this.img = null;
    },
    activate: function($super, $args) {
        this.title = "Floor Plan";
        $("floorplan").show();
        $super($args);
        this.refresh();
    },
    update: function($super, $args) {
        $super($args);
        this.refresh();
    },
    refresh: function() {
        var $floorplan = $("floorplan");
        while($floorplan.childNodes.length>0)
            $floorplan.removeChild($floorplan.childNodes[0]);
        this.building = null;
        this.floor = null;
        this.device = null;
        var $obj = this;
        Reactivity.buildingsGet(function($buildings){$obj.callback($buildings);});
    },
    callback: function($buildings) {
        if(this.getArgs()==null || this.getArgs().size()<1) {
            Pages.gotoPage(BuildingsPage, null);
            return;
        }
        if(this.getArgs().size()<2) {
            Pages.gotoPage(BuildingPage, this.getArgs());
            return;
        }
        if(!$buildings) {
            Pages.gotoPage(HomePage, null);
            return;
        }
        var $building = $buildings.get(this.getArgs()[0]);
        if(!$building) {
            Pages.gotoPage(BuildingsPage, null);
            return;
        }
        var $level = parseInt(this.getArgs()[1]);
        var $floor = null;        
        for(var $i=0;$i<$building.getFloors().size();$i++) {
            if($building.getFloors()[$i].getLevel() == $level) {
                $floor = $building.getFloors()[$i];
                break;
            }
        }
        if(!$floor) {
            Pages.gotoPage(BuildingPage, [$building.getGuid()]);
            return;
        }
        this.building = $building;
        this.floor = $floor;
        
        this.title = $floor.getName();
        Pages.setTitle(this.getTitle());
        
        var $floorplan = $("floorplan");
        while($floorplan.childNodes.length>0)
            $floorplan.removeChild($floorplan.childNodes[0]);
        this.img = new Image();
        this.img.src = "/Handler/Resource/GetStream.ashx?type=image/png&guid="+this.floor.getResource();
        var $img = document.createElement("img");
        $img.setAttribute("alt","");
        $img.setAttribute("src",this.img.src);
        var $obj = this;
        $img.addEventListener("error", function() {alert("Failed to retrieve floorplan"); $obj.gotoFloor();}, false);
        $img.addEventListener("abort", function() {alert("Failed to retrieve floorplan"); $obj.gotoFloor();}, false);
        $img.addEventListener("load", function() {window.scrollTo(0,0);Reactivity.devicesList(function($devices){$obj.floorplanCallback($devices);});}, false);
        $img.addEventListener("click", function() {$obj.goBack();}, false);
        $floorplan.appendChild($img);
        $floorplan.show();
    },
    floorplanCallback: function($devices) {
        if(!this.floor || !$devices) {
            this.gotoPage(BuildingsPage,null);
            return;
        }
        if(this.getArgs().size()>2)
            this.device = $devices.get(this.getArgs()[2]);

        var $floorplan = $("floorplan");
        var $devices_list = $devices.values();
        for(var $i=0;$i<$devices_list.size();$i++) {
            if($devices_list[$i].getBuilding() != this.floor.getBuilding()) continue;
            if($devices_list[$i].getFloor() != this.floor.getLevel()) continue;
            var $x = ($devices_list[$i].getPositionX()+1)*this.img.width/2 + 20; //plus margin
            var $y = ($devices_list[$i].getPositionY()+1)*this.img.height/2 + 20; //plus margin
            if($x<0)$x=0;
            if($y<0)$y=0;
            if($devices_list[$i]==this.device) {
                var $sx = $x-window.innerWidth/2;
                var $sy = $y-window.innerHeight/2;
                if($sx<0) $sx = 0;
                if($sy<0) $sy = 0;
                setTimeout(function(){window.scrollTo($sx,$sy);}, 200);
            }
            var $icon = null;
            if($devices_list[$i].getStatus()=="Online") {
                $icon = Util.DeviceType.getOnlineIcon($devices_list[$i].getType());
                if(!$icon) $icon = "images/icons/device_online.png";
            } else {
                $icon = Util.DeviceType.getOfflineIcon($devices_list[$i].getType());
                if(!$icon) $icon = "images/icons/device.png";
            }
            var $device_button = document.createElement("img");
            $device_button.setAttribute("class","pushpin");
            $device_button.setAttribute("src",$icon);
            $device_button.setAttribute("title",$devices_list[$i].getName());
            $device_button.setAttribute("alt",$devices_list[$i].getName());
            $device_button.setAttribute("guid",$devices_list[$i].getGuid());
            $device_button.style.pixelTop = $y - 24; //minus half of the pointer size
            $device_button.style.pixelLeft = $x - 24; //minus half of the pointer size
            $device_button.addEventListener("click",function(){Pages.gotoPage(DevicePage,[this.getAttribute("guid")]);},false);
            $floorplan.appendChild($device_button);
        }
    },
    goBack: function() {
        if(this.device)
            Pages.gotoPage(DevicePage, [this.device.getGuid()]);
        else
            this.gotoFloor();
    },
    gotoFloor: function() {
        if(!this.floor) return;
        Pages.gotoPage(FloorPage, [this.floor.getBuilding(),this.floor.getLevel()]);
    },
    deactivate: function($super) {
        $("floorplan").hide();
        $super();
    }
}))();
Pages.addPage(FloorplanPage);


/*
 * Class DevicesPage
 * This is the user info screen
 */
var DevicesPage = new (Class.create(Page, {
    initialize: function($super) {
        $super("devices", "Devices", 1);
    },
    activate: function($super, $args) {
        this.title = "Devices";
        Pages.getContent().appendChild($("page_devices"));
        Pages.setBackButton("Home", HomePage, null);
        $super($args);
        this.refresh();
    },
    refresh: function() {
        DevicesFilter.populate($("devices_list"), null, null);
    }
}))();
Pages.addPage(DevicesPage);


/*
 * Class DevicePage
 * This is the user info screen
 */
var DevicePage = new (Class.create(Page, {
    initialize: function($super) {
        $super("device", "Device", 1);
        this.building = null;
        this.floor = null;
        this.device = null;
        this.livechart = null;
        this.livesource1 = new LiveChartSource(61000,"rgb(30,65,115)",2);
        this.livesource2 = new LiveChartSource(61000,"rgb(0,100,50)",1);
        this.livesource3 = new LiveChartSource(61000,"rgb(255,125,0)",1);
        this.livesource4 = new LiveChartSource(61000,"rgb(30,65,115)",1);
        this.statschart = null;
        this.statstimer = null;
        this.subscription = null;
        this.control_ac_toggle = null;
    },
    activate: function($super, $args) {
        this.title = "Device";
        var $obj = this;
        Pages.setBackButton("All", DevicesPage, null);
        Pages.getContent().appendChild($("page_device"));
        $super($args);
        this.refresh();
    },
    update: function($super, $args) {
        $super($args);
        this.unsubscribe();
        $("device_chart_type").value="";
        if(this.control_ac_toggle)
            this.control_ac_toggle.setState(null);
        this.chartTypeChanged();
        this.refresh();
    },
    refresh: function($super) {
        this.building = null;
        this.floor = null;
        this.device = null;
        $("device_chart").hide();
        $("device_control_ac").hide();
        $("device_control_computer").hide();
        $("device_info").hide();
        $("device_location").hide();
        $("device_progress").show();
        if(this.statstimer) {
            clearTimeout(this.statstimer);
            this.statstimer = null;
        }
        if(!this.livechart) {
            this.livechart = new LiveChart($("device_canvas"), 60000);
            this.livechart.addSource(this.livesource1);
            this.livechart.addSource(this.livesource2);
            this.livechart.addSource(this.livesource3);
            this.livechart.addSource(this.livesource4);
        }
        if(!this.statschart) this.statschart = new StatisticsChart($("device_canvas"));
        var $obj = this;
        Reactivity.devicesList(function($devices){$obj.devicesCallback($devices);});
    },
    devicesCallback: function($devices) {
        if(this.getArgs()==null || this.getArgs().size()<1) {
            Pages.gotoPage(DevicesPage, null);
            return;
        }
        var $device = $devices.get(this.getArgs()[0]);
        if($device==null) {
            Pages.gotoPage(DevicesPage, null);
            return;
        }
        this.device = $device;
        
        $("device_description").innerHTML = $device.getDescription();
        if($device.getStatus() == "Online") {
            var $icon = Util.DeviceType.getOnlineIcon($device.getType());
            $("device_type").innerHTML = "<img src=\""+$icon+"\" class=\"icon_right\"/>" + Util.DeviceType.getName($device.getType());
            $("device_name").innerHTML = "<img src=\""+$icon+"\" class=\"icon_title\"/>" + $device.getName();
            $("device_status").innerHTML = "<img src=\"images/icons/online.png\" class=\"icon_right\"/>" + $device.getStatus();
            if($device.getType()!=Util.DeviceType.RFIDReader && (
                Reactivity.userCurrent().hasSubscribePermission() ||
                Reactivity.userCurrent().hasStatsPermission())) {
                $("device_chart_type_subscribe").disabled=!Reactivity.userCurrent().hasSubscribePermission();
                $("device_chart_type_stats").disabled=!Reactivity.userCurrent().hasStatsPermission();
                this.chartTypeChanged();
                $("device_chart").show();
            } else {
                $("device_chart_type").value="";
                this.chartTypeChanged();
            }
            if($device.getType()==Util.DeviceType.ACNode && Reactivity.userCurrent().hasControlPermission()) {
                if(!this.control_ac_toggle) {
                    var $obj = this;
                    this.control_ac_toggle = new Toggle($("device_control_ac_toggle"),null,function($state){$obj.controlAC($state);});
                }
                $("device_control_ac").show();
            }
            else if($device.getType()==Util.DeviceType.ComputerNode && Reactivity.userCurrent().hasControlPermission()) {
                $("device_control_computer").show();
            }
        }
        else
        {
            var $icon = Util.DeviceType.getOfflineIcon($device.getType());
            $("device_type").innerHTML = "<img src=\""+$icon+"\" class=\"icon_right\"/>" + Util.DeviceType.getName($device.getType());
            $("device_name").innerHTML = "<img src=\""+$icon+"\" class=\"icon_title\"/>" + $device.getName();
            $("device_status").innerHTML = "<img src=\"images/icons/offline.png\" class=\"icon_right\"/>" + $device.getStatus();
        }
        $("device_progress").hide();
        $("device_info").show();
        this.title = $device.getName();
        Pages.setTitle(this.getTitle());
        
        var $obj = this;
        Reactivity.buildingsGet(function($buildings){$obj.buildingsCallback($buildings);});
    },
    buildingsCallback: function($buildings) {
        if(this.device==null) {
            Pages.gotoPage(DevicesPage, null);
            return;
        }
        var $building = $buildings.get(this.device.getBuilding());
        if($building==null) {
            Pages.gotoPage(DevicesPage, null);
            return;
        }
        var $level = this.device.getFloor();
        var $floor = null;
        for(var $i=0;$i<$building.getFloors().size();$i++) {
            if($building.getFloors()[$i].getLevel() == $level) {
                $floor = $building.getFloors()[$i];
                break;
            }
        }
        if($floor==null) {
            Pages.gotoPage(DevicesPage, null);
            return;
        }
        this.building = $building;
        this.floor = $floor;
        
        $("device_floor").innerHTML = $floor.getName();
        $("device_building").innerHTML = $building.getName();
        $("device_location").show();
    },
    deactivate: function() {
        this.unsubscribe();
        $("device_chart_type").value = "";
        this.chartTypeChanged();
        if(this.statstimer) {
            clearTimeout(this.statstimer);
            this.statstimer = null;
        }
    },
    gotoBuilding: function() {
        if(!this.building) return;
        Pages.gotoPage(BuildingPage, [this.building.getGuid()]);
    },
    gotoFloor: function() {
        if(!this.floor) return;
        Pages.gotoPage(FloorPage, [this.floor.getBuilding(),this.floor.getLevel()]);
    },
    gotoFloorplan: function() {
        if(!this.floor || !this.device) return;
        Pages.gotoPage(FloorplanPage, [this.floor.getBuilding(),this.floor.getLevel(),this.device.getGuid()]);
    },
    chartTypeChanged: function() {
        if(!this.device) return;
        $("device_data").hide();
        $("device_display").hide();
        var $type = $("device_chart_type").value;
        if($type=="live") {
            if(this.device.getStatus()=="Online")
                if(Reactivity.userCurrent().hasSubscribePermission()) {
                    this.subscribe(-1);
                    return;
                } else
                    alert("Sorry, you dont have the permission");
            $("device_chart_type").value = "";
            this.chartTypeChanged();
            return;
        }
        if(this.subscription)
            this.unsubscribe();
        if(this.statstimer) {
            clearTimeout(this.statstimer);
            this.statstimer = null;
        }
        if(!$type) return;
        if(!Reactivity.userCurrent().hasStatsPermission()) {
            alert("Sorry, you dont have the permission");
            $("device_chart_type").value = "";
            return;
        }
        if($type == "15m")
            this.showStatistics(-1, 900000, Util.StatisticsType.Minutely, 20000);
        else if($type == "30m")
            this.showStatistics(-1, 1800000, Util.StatisticsType.Minutely, 20000);
        else if($type == "1h")
            this.showStatistics(-1, 3600000, Util.StatisticsType.Minutely, 20000);
        else if($type == "6h")
            this.showStatistics(-1, 21600000, Util.StatisticsType.Minutely, 20000);
        else if($type == "12h")
            this.showStatistics(-1, 43200000, Util.StatisticsType.Minutely, 20000);
        else if($type == "1d")
            this.showStatistics(-1, 86400000, Util.StatisticsType.Hourly, 120000);
        else if($type == "3d")
            this.showStatistics(-1, 259200000, Util.StatisticsType.Hourly, 1200000);
        else if($type == "1w")
            this.showStatistics(-1, 604800000, Util.StatisticsType.Hourly, 1200000);
        else if($type == "2w")
            this.showStatistics(-1, 1209600000, Util.StatisticsType.Daily, 1200000);
        else if($type == "1m")
            this.showStatistics(-1, 2678400000, Util.StatisticsType.Daily, 1200000);
    
    },
    showStatistics: function($service, $timespan, $type, $interval) {
        if(this.statstimer) {
            clearTimeout(this.statstimer);
            this.statstimer = null;
        }
        if(!this.device) return;
        var $obj = this;
        var $end = new Date();
        var $start = new Date();
        $start.setTime($start.getTime()-$timespan);
        if(Reactivity.statisticsQuery(this.device.getGuid(), $service, $type, $start, $end, function($stats){$obj.showStatisticsCallback($stats, $start, $end);}))
            this.statstimer = setTimeout(function(){$obj.showStatistics($service,$timespan,$type,$interval);},$interval);
        else {
            alert("Failed to retrieve statistics");
            $("device_chart_type").value="";
            this.chartTypeChanged();
        }
    },
    showStatisticsCallback: function($stats, $start, $end) {
        if(!$stats) {
            alert("Failed to retrieve statistics");
            $("device_chart_type").value="";
            this.chartTypeChanged();
            return;
        }
        if($stats.size()<=0) return;
        this.statschart.clear();
        var $graph1 = new StatisticsChartSource($start,$end,"rgb(30,65,115)",2);
        var $graph2 = new StatisticsChartSource($start,$end,"rgb(0,100,50)",2);
        var $graph3 = new StatisticsChartSource($start,$end,"rgb(255,125,0)",2);
        var $graph4 = new StatisticsChartSource($start,$end,"rgb(30,65,115)",2);
        this.statschart.addSource($graph1);
        this.statschart.addSource($graph2);
        this.statschart.addSource($graph3);
        this.statschart.addSource($graph4);
        for(var $i=0;$i<$stats.size();$i++) {
            if($stats[$i].getService() == Util.ServiceType.Default)
                $graph1.push($stats[$i]);
            else if($stats[$i].getService() == Util.ServiceType.ComputerNode_Memory)
                $graph2.push($stats[$i]);
            else if($stats[$i].getService() == Util.ServiceType.AccelerationSensor_Y)
                $graph3.push($stats[$i]);
            else if($stats[$i].getService() == Util.ServiceType.AccelerationSensor_Z)
                $graph4.push($stats[$i]);
        }
        this.statschart.redraw();
        $("device_display").show();
    },
    subscribe: function($service) {
        if(!this.device) return;
        if(this.device.getStatus()!="Online") return;
        if(!this.subscription) {
            var $obj = this;
            Reactivity.subscribe(
                this.device.getGuid(), $service,
                function($subscription){$obj.subscribeCallback($subscription);},
                function($subscription){$obj.subscriptionUpdated($subscription);},
                function($subscription, $timestamp, $service, $type, $value){$obj.subscriptionNotified($subscription, $timestamp, $service, $type, $value);});
        } else {
            LiveChartUpdater.begin(this.livechart);
        }
    },
    subscribeCallback: function($subscription) {
        if(!$subscription) {
            alert("Failed to subscribe");
            $("device_chart_type").value="";
            this.chartTypeChanged();
            return;
        }
        this.subscription = $subscription;
        $("device_display").show();
        LiveChartUpdater.begin(this.livechart);
    },
    unsubscribe: function() {
        this.subscription = null;
        LiveChartUpdater.end();
        Reactivity.unsubscribe();
        this.livechart.clear();
        this.statschart.clear();
    },
    subscriptionUpdated: function($subscription) {
        if(!this.subscription || !$subscription || this.subscription.getGuid()!=$subscription.getGuid()) return;
        if($subscription.getStatus()=="Stopped") {
            $("device_chart_type").value="";
            this.chartTypeChanged();
        }
    },
    subscriptionNotified: function($subscription, $timestamp, $service, $type, $value) {
        if(!this.subscription || !$subscription || this.subscription.getGuid()!=$subscription.getGuid()) return;
        if($service == Util.ServiceType.ACNode_Power) {
            this.livesource1.push($timestamp,$value);
            $("device_data").innerHTML = $value;
            $("device_data").show();
        }
        else if($service == Util.ServiceType.ACNode_Current)
            this.livesource2.push($timestamp,$value);
        else if($service == Util.ServiceType.ACNode_Voltage)
            this.livesource3.push($timestamp,$value);
        else if($service == Util.ServiceType.AccelerationSensor_Z)
            this.livesource4.push($timestamp,$value);
        if($service == Util.ServiceType.ACNode_Relay && this.device.getType()==Util.DeviceType.ACNode && this.control_ac_toggle)
            this.control_ac_toggle.setState(($value)?true:false);
    },
    controlAC: function($value) {
        if(!this.device) return;
        if(this.device.getType()!=Util.DeviceType.ACNode) return;
        if(this.device.getStatus()!="Online") return;
        Reactivity.dataSend(this.device.getGuid(), Util.ServiceType.ACNode_Relay, Util.DataType.Bool, ($value)?"true":"false", function($state){if($state!="OK")alert("Failed to send data.");});
    },
    controlComputer: function($value) {
        if(!$value) return;
        if(!this.device) return;
        if(this.device.getType()!=Util.DeviceType.ComputerNode) return;
        if(this.device.getStatus()!="Online") return;
        Reactivity.dataSend(this.device.getGuid(), Util.ServiceType.Default, Util.DataType.Short, $value, function($state){if($state!="OK")alert("Failed to send data.");});
    }
}))();
Pages.addPage(DevicePage);


