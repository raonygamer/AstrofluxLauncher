package {
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
public class AstrofluxDesktop extends Sprite {
	private const SWF_SOURCE:String = "http://r.playerio.com/r/rymdenrunt-k9qmg7cvt0ylialudmldvg/Preload.swf";
	private const VERSION:int = 1;

	private var loader:Loader = new Loader();
	private var loaderContext:LoaderContext = new LoaderContext();
	private var childSWF:DisplayObject;

	private var popup:Popup;
	private var tf:TextField = new TextField();
	private var loadingImageCenter:Point;
	private var bgWidth:Number;
	private var bgHeight:Number;

	private var background:Bitmap = new BackgroundBitmap();
	private var loadingImage:Bitmap = new LoadingBitmap();

	public function AstrofluxDesktop() {
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

		tf.x = 160;
		tf.y = 0;
		tf.width = stage.stageWidth - tf.x;
		tf.height = stage.stageHeight;
		tf.setTextFormat(new TextFormat("Verdana", 24, 0xffffff));

		NativeApplication.nativeApplication.addEventListener("exiting", onExit);
		try {
			loadExternalSWF();
		} catch (e:Error) {
			log("*** ERROR ***");
			log(e.message);
			log(e.getStackTrace());
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

	private function onLoadComplete(e:Event):void {
		var urlLoader:URLLoader = e.target as URLLoader;

		log("Load Complete");
		loader.loadBytes(ByteArray(urlLoader.data), loaderContext);
		urlLoader.removeEventListener("complete", onLoadComplete);
		urlLoader.removeEventListener("ioError", onLoadError);
	}

	private function onIoError(e:Event):void {
		log("IOErrorEvent.IO_ERROR: " + e.toString());
	}

	private function loadExternalSWF():void {
		var urlLoader:URLLoader = new URLLoader();
		var launcherConfigFile:File = new File(File.userDirectory.nativePath + "/AppData/Roaming/AstrofluxLauncher/config.json");
		var swfSource:String = SWF_SOURCE;

		var loadSwf:Function = function (url:String):void {
			var request:URLRequest = new URLRequest(url);
			log("Loading external swf...");
			urlLoader.load(request);
			urlLoader.dataFormat = "binary";
			urlLoader.addEventListener("complete", onLoadComplete);
			urlLoader.addEventListener("ioError", onLoadError);
		};

		var onHttpStatus:Function = function (e:HTTPStatusEvent):void {
			loadSwf(e.status == 200 ? swfSource : SWF_SOURCE);
		};

		if (launcherConfigFile.exists) {
			var fs:FileStream = new FileStream();
			try {
				fs.open(launcherConfigFile, FileMode.READ);
				var jsonConfig:Object = JSON.parse(fs.readUTFBytes(fs.bytesAvailable));
				fs.close();
				if ("SwfRemoteUrl" in jsonConfig) {
					var url:String = jsonConfig["SwfRemoteUrl"];
					if (url.search("file://") != -1) {
						var swfFile:File = new File(url.replace("file://", ""));
						if (swfFile.exists)
							swfSource = url;
						loadSwf(swfSource);
					} else if (url.search("http://") != -1) {
						var testRequest:URLRequest = new URLRequest(url);
						var testLoader:URLLoader = new URLLoader();
						testLoader.load(testRequest);
						testLoader.addEventListener(HTTPStatusEvent.HTTP_STATUS, onHttpStatus);
					} else {
						loadSwf(SWF_SOURCE);
					}
				}
			} catch (e:Error) {
				log(e.message);
				var request:URLRequest = new URLRequest(SWF_SOURCE);
				log("Loading external swf...");
				urlLoader.load(request);
				urlLoader.dataFormat = "binary";
				urlLoader.addEventListener("complete", onLoadComplete);
				urlLoader.addEventListener("ioError", onLoadError);
			}
		} else {
			loadSwf(SWF_SOURCE);
		}
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
		log("onUncaughtError: " + e.error.getStackTrace());
	}

	private function onChildLoadComplete():void {
		stage.removeEventListener("enterFrame", update);
		stage.nativeWindow.removeEventListener("resize", resize);
		createStarlingChild();
		log("created starling child");
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

	public static var Sended:Boolean = false;

	private function createStarlingChild():void {
		var domain:ApplicationDomain = loader.contentLoaderInfo.applicationDomain;
		if (domain.hasDefinition("RymdenRunt")) {
			var rymdenRuntClass:Class = domain.getDefinition("RymdenRunt") as Class;
			var data:Object = {
				"origin": "desktop",
				"version": VERSION
			};
			childSWF = new rymdenRuntClass(data);
		}
	}

	private function log(text:String):void {
		tf.appendText(text + "\n");
		tf.scrollV = tf.maxScrollV;
		trace(text);
	}

	private function onExit(e:Event = null):void {
		log("Exiting application, cleaning up");
		if (childSWF) {
			childSWF.removeEventListener("exitgame", onExit);
			removeChild(childSWF);
		}
		NativeApplication.nativeApplication.removeEventListener("exiting", onExit);
		NativeApplication.nativeApplication.exit();
	}
}
}