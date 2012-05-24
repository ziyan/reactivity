var Settings = new function() {
this.onLoad = function() {
	this.refresh();
};
this.onUnload = function() {
	Reactivity.close();
};
this.refresh = function() {
	$("settings").style.display="none";
	$("login").style.display="none";
	$("progress").style.display="block";
	var obj = this;
	Reactivity.userCheckStatus(function(status){obj.userCallback(status);});
};
this.onClosing = function(e) {
	if (e.closeAction == e.Action.commit) 
		this.saveSettings();
	e.cancel = false;
};
this.saveSettings = function() {
	if($("device").value)
		System.Gadget.Settings.writeString("device",$("device").value);
	else
		System.Gadget.Settings.writeString("device","");
	if($("type").value)
		System.Gadget.Settings.writeString("type",$("type").value);
	else
		System.Gadget.Settings.writeString("type","");
};
this.logout = function() {
	$("progress").style.display="block";
	$("settings").style.display="none";
	Reactivity.userLogout();
	this.refresh();
};
this.login = function() {
	var service_uri = $("service_uri").value;
	var username = $("username").value;
	var password = $("password").value;
	if(!service_uri || !username || !password) {
		$("message").innerHTML="Please fill in all blanks.";
		return;
	}
	password = Util.hash(password);
	$("login").style.display="none";
	$("progress").style.display="block";
	var obj = this;
	Reactivity.userLogin(service_uri,username,password,function(status){obj.userCallback(status);});
};
this.userCallback = function(status) {
	if(status=="OK" && Reactivity.userCurrent()) {
		$("info").innerHTML="Logged in as "+(Reactivity.userCurrent().getName())+".";
		this.listDevices();
		return;
	}
	$("progress").style.display="none";
	$("login").style.display="block";
	if(status=="ConnectionFailed") {
		$("message").innerHTML="Cannot find server";
		return;
	}
	if(status=="NotLoggedIn") {
		$("message").innerHTML="";
		return;
	}
	if(status=="InvalidPassword") {
		$("message").innerHTML="Incorrect password";
		return;
	}
	$("message").innerHTML="Error: "+status;
};
this.listDevices = function() {
	var obj = this;
	Reactivity.deviceList(function(devices){obj.listDevicesCallback(devices);});
};
this.listDevicesCallback = function(devices) {
	while($("device").childNodes.length>0)
        $("device").removeChild($("device").childNodes[0]);
	for(var i=0;i<devices.length;i++) {
		if(devices[i].getType()==Util.DeviceType.RFIDReader) continue;
		var device = document.createElement("option");
		device.setAttribute("value",devices[i].getGuid());
		if(System.Gadget.Settings.readString("device")==devices[i].getGuid())
			device.setAttribute("selected","selected");
		device.innerHTML=devices[i].getName() + (devices[i].getStatus()=="Online"?" (Online)":"");
		$("device").appendChild(device);
	}
	$("type").value = System.Gadget.Settings.readString("type");
	$("progress").style.display="none";
	$("settings").style.display="block";
};
};
System.Gadget.onSettingsClosing = function(e){Settings.onClosing(e);};