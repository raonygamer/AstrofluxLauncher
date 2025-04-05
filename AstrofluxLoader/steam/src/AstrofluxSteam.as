package {
import com.amanitadesign.steam.FRESteamWorks;
import com.amanitadesign.steam.MicroTxnAuthorizationResponse;
import com.amanitadesign.steam.SteamEvent;

import flash.desktop.NativeApplication;
import flash.display.Bitmap;
import flash.display.DisplayObject;
import flash.display.Loader;
import flash.display.LoaderInfo;
import flash.display.Sprite;
import flash.events.ErrorEvent;
import flash.events.Event;
import flash.events.HTTPStatusEvent;
import flash.events.InvokeEvent;
import flash.events.KeyboardEvent;
import flash.events.NativeWindowBoundsEvent;
import flash.events.TimerEvent;
import flash.events.UncaughtErrorEvent;
import flash.filesystem.File;
import flash.filesystem.FileMode;
import flash.filesystem.FileStream;
import flash.geom.Matrix;
import flash.geom.Point;
import flash.net.URLLoader;
import flash.net.URLRequest;
import flash.system.ApplicationDomain;
import flash.system.LoaderContext;
import flash.text.TextField;
import flash.text.TextFormat;
import flash.ui.Keyboard;
import flash.utils.ByteArray;
import flash.utils.Timer;

[SWF(width="800", height="600", backgroundColor="#000000", frameRate="60")]
public class AstrofluxSteam extends Sprite {
	private const LAUNCHER_CONFIG_FILE:File = new File(File.userDirectory.nativePath + "/AppData/Roaming/AstrofluxLauncher/config.json");

	private var loader:Loader = new Loader();
	private var loaderContext:LoaderContext = new LoaderContext();
	private var childSWF:DisplayObject;

	private var popup:Popup;
	private var loadingImageCenter:Point;
	private var bgWidth:Number;
	private var bgHeight:Number;

	private var background:Bitmap = new BackgroundBitmap();
	private var loadingImage:Bitmap = new LoadingBitmap();

	public var steamworks:FRESteamWorks = new FRESteamWorks();
	private var log:Log = Log.create(new File(File.documentsDirectory.nativePath + "/astroflux/steam_loader.log"));

	public function AstrofluxSteam() {
		super();
		NativeApplication.nativeApplication.addEventListener("invoke", onInvoke);
		loaderContext.allowCodeImport = true;
		loaderContext.allowLoadBytesCodeExecution = true;
		loader.contentLoaderInfo.addEventListener("complete", onContentLoadingComplete);
		loader.contentLoaderInfo.addEventListener("ioError", onIoError);
		loader.uncaughtErrorEvents.addEventListener("uncaughtError", onUncaughtError);
	}

	private function onContentLoadingComplete(e:Event):void {
		var contentLoaderInfo:LoaderInfo = e.target as LoaderInfo;
		contentLoaderInfo.removeEventListener("complete", onContentLoadingComplete);
		onChildLoadComplete();
	}

	public function onInvoke(e:InvokeEvent):void {
		NativeApplication.nativeApplication.removeEventListener("invoke", onInvoke);
		addChild(background);
		addChild(loadingImage);
		bgWidth = background.width;
		bgHeight = background.height;
		loadingImage.alpha = 0.7;
		loadingImage.smoothing = true;
		stage.scaleMode = "noScale";
		stage.align = "TL";
		stage.nativeWindow.addEventListener("resize", resize);
		stage.addEventListener("enterFrame", update);
		stage.addEventListener("keyDown", onKeyDown);
		stage.displayState = "fullScreenInteractive";
		steamworks.addEventListener(SteamEvent.STEAM_RESPONSE, handleSteamEvent);
		steamworks.addOverlayWorkaround(stage, true);
		NativeApplication.nativeApplication.addEventListener("exiting", onExit);
		try {
			processCommandLine(e);
			if (!steamworks.init()) {
				showPopup("Steam is not running.");
				return;
			}
			loadExternalSWF();
		} catch (e:Error) {
			log.debug("*** ERROR ***");
			log.debug(e.message);
			log.debug(e.getStackTrace());
			showPopup("Failed to load game client.");
		}
	}

	public function onKeyDown(keyEvent:KeyboardEvent):void {
		if (keyEvent.keyCode == Keyboard.ESCAPE) {
			keyEvent.preventDefault();
		}
	}

	public function resize(resizeEvent:NativeWindowBoundsEvent):void {
		background.width = stage.stageWidth;
		background.height = background.width * bgHeight / bgWidth;
		if (background.width > bgWidth) {
			background.width = bgWidth;
			background.height = bgHeight;
			background.x = stage.stageWidth / 2 - background.width / 2;
		}
		if (background.height < stage.stageHeight) {
			background.y = stage.stageHeight / 2 - background.height / 2;
		} else {
			background.y = 0;
		}
		loadingImage.rotation = 0;
		loadingImage.x = stage.stageWidth / 2 - loadingImage.width / 2;
		loadingImage.y = stage.stageHeight / 2 - loadingImage.height / 2;
		loadingImageCenter = new Point(loadingImage.x + loadingImage.width / 2, loadingImage.y + loadingImage.height / 2);
		if (popup) {
			popup.x = stage.stageWidth / 2 - popup.width / 2;
			popup.y = stage.stageHeight / 2 - popup.height / 2;
		}
	}

	private function update(e:Event):void {
		if (!loadingImageCenter) {
			return;
		}

		// Get the current transformation matrix of the loading image
		var loadingImageMatrix:Matrix = loadingImage.transform.matrix;

		// Translate the matrix to the origin
		loadingImageMatrix.tx -= loadingImageCenter.x;
		loadingImageMatrix.ty -= loadingImageCenter.y;

		// Rotate the matrix by a small angle (0.5 degrees)
		var rotationAngle:Number = 0.5 * (Math.PI / 180.0); // 0.5 degrees in radians
		loadingImageMatrix.rotate(rotationAngle);

		// Translate the matrix back to the original position
		loadingImageMatrix.tx += loadingImageCenter.x;
		loadingImageMatrix.ty += loadingImageCenter.y;

		// Apply the transformed matrix back to the loading image
		loadingImage.transform.matrix = loadingImageMatrix;
	}

	private function showPopup(messageString:String):void {
		if (popup) {
			return;
		}
		popup = new Popup(messageString, function ():void {
			removeChild(popup);
			onExit();
		});
		popup.x = stage.stageWidth / 2 - popup.width / 2;
		popup.y = stage.stageHeight / 2 - popup.height / 2;
		addChild(popup);
	}

	private function onLoadError(e:Event):void {
		showPopup("Failed to load game client.");
	}

	private function loadSwfBytes(bytes:ByteArray):void {
		log.debug("Loading SWF bytes...");
		loader.loadBytes(bytes, loaderContext);
	}

	private function onLoadComplete(e:Event):void {
		var urlLoader:URLLoader = e.target as URLLoader;

		log.debug("Load Complete");
		loadSwfBytes(ByteArray(urlLoader.data));
		urlLoader.removeEventListener("complete", onLoadComplete);
		urlLoader.removeEventListener("ioError", onLoadError);
	}

	private function onIoError(e:Event):void {
		log.debug("IOErrorEvent.IO_ERROR: " + e.toString());
	}

	private function loadLocalSwf(file:File):Boolean {
		if (!file.exists) {
			log.debug("File does not exist: " + file.nativePath);
			return false;
		}

		var fs:FileStream = new FileStream();
		try {
			log.debug("Loading local SWF from: " + file.nativePath);
			fs.open(file, FileMode.READ);
			var bytes:ByteArray = new ByteArray();
			fs.readBytes(bytes);
			loadSwfBytes(bytes);
			return true;
		} catch (e:Error) {
			log.debug("Error loading local SWF: " + e.message);
			log.debug(e.getStackTrace());
		} finally {
			fs.close();
		}
		return false;
	}

	private function loadRemoteSwf(url:String):void {
		var urlLoader:URLLoader = new URLLoader();
		var request:URLRequest = new URLRequest(url);
		log.debug("Loading remote SWF from: " + url);
		urlLoader.load(request);
		urlLoader.dataFormat = "binary";
		urlLoader.addEventListener("complete", onLoadComplete);
		urlLoader.addEventListener("ioError", onLoadError);
	}

	private function loadVanillaSwf():void {
		loadRemoteSwf("http://r.playerio.com/r/rymdenrunt-k9qmg7cvt0ylialudmldvg/Preload.swf");
	}

	private function onTestHttpStatus(e:HTTPStatusEvent, url:String):void {
		log.debug("Test HTTP Status for: '" + url + "' is " + e.status);
		if (e.status == 200) {
			loadRemoteSwf(url);
		} else {
			log.debug("Test HTTP Status was not OK: " + e.status);
			loadVanillaSwf();
		}
	}

	private function loadExternalSWF():void {
		var appDirectory:File = File.applicationDirectory;
		var files:Array = appDirectory.getDirectoryListing();
		var currentFileName:String = PathUtils.getFileName(this.loaderInfo.url);
		for each (var file:File in files) {
			if (file.isDirectory || file.name == currentFileName || file.extension.toLowerCase() != "swf")
				continue;
			if (loadLocalSwf(file))
				return;
		}

		if (!LAUNCHER_CONFIG_FILE.exists) {
			log.debug("Launcher config file does not exist: " + LAUNCHER_CONFIG_FILE.nativePath);
			loadVanillaSwf();
			return;
		}

		var fs:FileStream = new FileStream();
		try {
			fs.open(LAUNCHER_CONFIG_FILE, FileMode.READ);
			var jsonConfig:Object = JSON.parse(fs.readUTFBytes(fs.bytesAvailable));
			if ("SwfRemoteUrl" in jsonConfig) {
				var url:String = jsonConfig["SwfRemoteUrl"];
				if (url.search("file://") != -1) {
					var swfFile:File = new File(url.replace("file://", ""));
					if (!swfFile.exists) {
						log.debug("SwfRemoteUrl file does not exist: " + swfFile.nativePath);
						loadVanillaSwf();
						return;
					}
					if (loadLocalSwf(swfFile))
						return;
				} else if (url.search("http://") != -1 || url.search("https://") != -1) {
					var testRequest:URLRequest = new URLRequest(url);
					var testLoader:URLLoader = new URLLoader();
					testLoader.load(testRequest);

					var testLoaderCallback:Function = function(e:HTTPStatusEvent):void {
						onTestHttpStatus(e, url);
						(e.target as URLLoader).removeEventListener(HTTPStatusEvent.HTTP_STATUS, testLoaderCallback);
					};

					testLoader.addEventListener(HTTPStatusEvent.HTTP_STATUS, testLoaderCallback);
					return;
				}
			}
		} catch (e:Error) {
			log.debug(e.message);
		} finally {
			fs.close();
		}
		loadVanillaSwf();
	}

	private function onUncaughtError(e:UncaughtErrorEvent):void {
		e.preventDefault();
		e.stopImmediatePropagation();
		e.stopPropagation();
		var error:String = "onUncaughtError : ";
		if (e.error is Error) {
			error += !!e.error.getStackTrace() ? e.error.getStackTrace() : e.error.message;
		} else if (e.error is ErrorEvent) {
			error += e.error.text;
		} else {
			error += e.error.toString();
		}
		log.debug("onUncaughtError: " + e.error.getStackTrace());
	}

	private function onChildLoadComplete():void {
		stage.removeEventListener("enterFrame", update);
		stage.nativeWindow.removeEventListener("resize", resize);
		createStarlingChild();
		log.debug("created starling child");
		addChild(childSWF);
		childSWF.addEventListener("exitgame", onExit);
		delayedFunctionCall(10 * 60, function ():void {
			removeChild(loadingImage);
			removeChild(background);
			loadingImage = null;
			background = null;
		});
	}

	private function delayedFunctionCall(delay:int, func:Function):void {
		var timer:Timer = new Timer(delay, 1);
		var onTimer:Function = function (e:Event):void {
			timer.removeEventListener(TimerEvent.TIMER, onTimer);
			func();
		}

		timer.addEventListener(TimerEvent.TIMER, onTimer);
		timer.start();
	}

	private function createStarlingChild():void {
		var domain:ApplicationDomain = loader.contentLoaderInfo.applicationDomain;
		if (domain.hasDefinition("RymdenRunt")) {
			var rymdenRuntClass:Class = domain.getDefinition("RymdenRunt") as Class;
			var data:Object = {
				"origin": "steam",
				"userId": steamworks.getUserID(),
				"appId": steamworks.getAppID().toString(),
				"name": steamworks.getPersonaName(),
				"language": steamworks.getCurrentGameLanguage(),
				"ticket": getAuthTicketString()
			};
			childSWF = new rymdenRuntClass(data);
		}
	}

	private function processCommandLine(e:InvokeEvent):void {
		log.debug("Arguments: " + e.arguments);
		for (var i:int = 0; i < e.arguments.length; i++) {
			var appIdArg:String = e.arguments[i];
			if ("-appid" !== appIdArg) {
				trace("FRESteamWorksTest called with invalid argument: " + appIdArg);
				NativeApplication.nativeApplication.exit(1);
			} else {
				i++;
				testRestartAppIfNecessary(e.arguments[i]);
			}
		}
	}

	public function testRestartAppIfNecessary(code:uint):void {
		if (!code) {
			log.debug("FRESteamWorkTest: -appid requires argument");
			NativeApplication.nativeApplication.exit(1);
		}

		if (steamworks.restartAppIfNecessary(code)) {
			log.debug("App started outside of Steam with no app_id.txt: Steam will restart");
			NativeApplication.nativeApplication.exit(0);
		}
	}

	private function getAuthTicketString():String {
		if (!steamworks.isReady) {
			return null;
		}

		var ticketBuffer:ByteArray = new ByteArray();
		steamworks.getAuthSessionTicket(ticketBuffer);
		var ticket:String = "";
		for (var i:int = 0; i < ticketBuffer.length; i++) {
			var hexDigit:String = ticketBuffer[i].toString(16);
			ticket += (hexDigit.length < 2 ? "0" : "") + hexDigit;
		}
		return ticket;
	}

	private function handleSteamEvent(e:SteamEvent):void {
		var response:MicroTxnAuthorizationResponse = null;
		if (e.req_type == 28) {
			while (true) {
				response = steamworks.microTxnResult();
				if (response == null) {
					break;
				}

				if (response.authorized) {
					childSWF.dispatchEvent(new Event("steambuysuccess"));
				} else {
					childSWF.dispatchEvent(new Event("steambuyfail"));
				}
			}
		}
		log.debug("Incoming: " + e.req_type);
	}

	private function onExit(e:Event = null):void {
		log.debug("Exiting application, cleaning up");
		if (childSWF) {
			childSWF.removeEventListener("exitgame", onExit);
			removeChild(childSWF);
		}
		if (steamworks) {
			steamworks.removeEventListener(SteamEvent.STEAM_RESPONSE, handleSteamEvent);
			steamworks.dispose();
		}
		NativeApplication.nativeApplication.removeEventListener("exiting", onExit);
		NativeApplication.nativeApplication.exit();
	}
}
}
