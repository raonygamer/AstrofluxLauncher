package {
	import flash.desktop.NativeApplication;
	import flash.display.Bitmap;
	import flash.display.DisplayObject;
	import flash.display.Loader;
	import flash.display.Sprite;
	import flash.events.ErrorEvent;
	import flash.events.Event;
	import flash.events.InvokeEvent;
	import flash.events.KeyboardEvent;
	import flash.events.NativeWindowBoundsEvent;
	import flash.events.UncaughtErrorEvent;
	import flash.geom.Matrix;
	import flash.geom.Point;
	import flash.net.URLLoader;
	import flash.net.URLRequest;
	import flash.system.ApplicationDomain;
	import flash.system.LoaderContext;
	import flash.text.TextField;
	import flash.text.TextFormat;
	import flash.utils.ByteArray;
	import flash.utils.Timer;
	
	public class AstrofluxDesktop extends Sprite {
		private const SWF_URL:String = "@PRELOAD_SWF_URL@";
		private const CLASSNAME:String = "RymdenRunt";
		private const VERSION:int = 1;
		private const DEBUG:Boolean = false;
		private var tf:TextField;
		private var loadingImageCenter:Point;
		private var bgWidth:Number;
		private var bgHeight:Number;
		private var loader:Loader;
		private var childSWF:DisplayObject;
		public var BG:Class = bg_jpg$a7c0ea5b86b7b87933c4ba05b43fc8801689124682;
		private var background:Bitmap = new BG();
		public var Loading:Class = loading_png$3db4966a53dd72fedc5843641e385e531471585260;
		private var loadingImage:Bitmap = new Loading();
		private var popup:Popup;
		
		public function AstrofluxDesktop() {
			super();
			NativeApplication.nativeApplication.addEventListener("invoke",onInvoke);
		}
		
		public function onInvoke(param1:InvokeEvent) : void {
			NativeApplication.nativeApplication.removeEventListener("invoke",onInvoke);
			addChild(background);
			addChild(loadingImage);
			bgWidth = background.width;
			bgHeight = background.height;
			loadingImage.alpha = 0.7;
			loadingImage.smoothing = true;
			stage.scaleMode = "noScale";
			stage.align = "TL";
			stage.nativeWindow.addEventListener("resize",resize);
			stage.addEventListener("enterFrame",update);
			stage.addEventListener("keyDown",onKeyDown);
			stage.displayState = "fullScreenInteractive";
			tf = new TextField();
			tf.x = 160;
			tf.width = stage.stageWidth - tf.x;
			tf.height = stage.stageHeight;
			tf.setTextFormat(new TextFormat("Verdana",18,0xffffff));
			NativeApplication.nativeApplication.addEventListener("exiting",onExit);
			try {
				loadExternalSWF();
			}
			catch(e:Error) {
				log("*** ERROR ***");
				log(e.message);
				log(e.getStackTrace());
				showPopup("Failed to load game client.");
			}
		}
		
		public function onKeyDown(param1:KeyboardEvent) : void {
			if(param1.keyCode == 27) {
				param1.preventDefault();
			}
		}
		
		public function resize(param1:NativeWindowBoundsEvent) : void {
			background.width = stage.stageWidth;
			background.height = background.width * bgHeight / bgWidth;
			if(background.width > bgWidth) {
				background.width = bgWidth;
				background.height = bgHeight;
				background.x = stage.stageWidth / 2 - background.width / 2;
			}
			if(background.height < stage.stageHeight) {
				background.y = stage.stageHeight / 2 - background.height / 2;
			} else {
				background.y = 0;
			}
			loadingImage.rotation = 0;
			loadingImage.x = stage.stageWidth / 2 - loadingImage.width / 2;
			loadingImage.y = stage.stageHeight / 2 - loadingImage.height / 2;
			loadingImageCenter = new Point(loadingImage.x + loadingImage.width / 2,loadingImage.y + loadingImage.height / 2);
			if(popup) {
				popup.x = stage.stageWidth / 2 - popup.width / 2;
				popup.y = stage.stageHeight / 2 - popup.height / 2;
			}
		}
		
		private function update(param1:Event) : void {
			if(!loadingImageCenter) {
				return;
			}
			var _loc2_:Matrix = loadingImage.transform.matrix;
			_loc2_.tx -= loadingImageCenter.x;
			_loc2_.ty -= loadingImageCenter.y;
			_loc2_.rotate(0.008726646259971648);
			_loc2_.tx += loadingImageCenter.x;
			_loc2_.ty += loadingImageCenter.y;
			loadingImage.transform.matrix = _loc2_;
		}
		
		private function showPopup(param1:String) : void {
			var popup:Popup;
			var message:String = param1;
			if(popup) {
				return;
			}
			popup = new Popup(message,function():void {
				removeChild(popup);
				onExit();
			});
			popup.x = stage.stageWidth / 2 - popup.width / 2;
			popup.y = stage.stageHeight / 2 - popup.height / 2;
			addChild(popup);
		}
		
		private function loadExternalSWF() : void {
			var loaderContext:LoaderContext;
			var urlLoader:URLLoader;
			var request:URLRequest;
			var loadError:* = function(param1:Event):void {
				showPopup("Failed to load game client.");
			};
			var loadComplete:* = function(param1:Event):void {
				log("load complete");
				log(urlLoader.data.length);
				loader.loadBytes(ByteArray(urlLoader.data),loaderContext);
				urlLoader.removeEventListener("complete",loadComplete);
				urlLoader.removeEventListener("ioError",loadError);
			};
			log("Loading external swf...");
			loader = new Loader();
			loaderContext = new LoaderContext();
			loaderContext.allowCodeImport = true;
			loaderContext.allowLoadBytesCodeExecution = true;
			loader.contentLoaderInfo.addEventListener("complete",(function():* {
				var onContentLoaderInfo:Function;
				return onContentLoaderInfo = function(param1:Event):void {
					loader.contentLoaderInfo.removeEventListener("complete",onContentLoaderInfo);
					onChildLoadComplete();
				};
			})());
			loader.contentLoaderInfo.addEventListener("ioError",function(param1:Event):void {
				log("IOErrorEvent.IO_ERROR: " + param1.toString());
			});
			loader.uncaughtErrorEvents.addEventListener("uncaughtError",onUncaughtError);
			urlLoader = new URLLoader();
			request = new URLRequest("@PRELOAD_SWF_URL@");
			urlLoader.load(request);
			urlLoader.dataFormat = "binary";
			urlLoader.addEventListener("complete",loadComplete);
			urlLoader.addEventListener("ioError",loadError);
		}
		
		private function onUncaughtError(param1:UncaughtErrorEvent) : void {
			param1.preventDefault();
			param1.stopImmediatePropagation();
			param1.stopPropagation();
			var _loc2_:String = "onUncaughtError : ";
			if(param1.error is Error) {
				_loc2_ += !!param1.error.getStackTrace() ? param1.error.getStackTrace() : param1.error.message;
			} else if(param1.error is ErrorEvent) {
				_loc2_ += param1.error.text;
			} else {
				_loc2_ += param1.error.toString();
			}
			log("onUncaughtError: " + param1.error.getStackTrace());
		}
		
		private function onChildLoadComplete() : void {
			stage.removeEventListener("enterFrame",update);
			stage.nativeWindow.removeEventListener("resize",resize);
			createStarlingChild();
			log("created starling child");
			addChild(childSWF);
			childSWF.addEventListener("exitgame",onExit);
			delayedFunctionCall(10 * 60,function():void {
				removeChild(loadingImage);
				removeChild(background);
				loadingImage = null;
				background = null;
			});
		}
		
		private function delayedFunctionCall(param1:int, param2:Function) : void {
			var delay:int = param1;
			var func:Function = param2;
			var timer:Timer = new Timer(delay,1);
			timer.addEventListener("timer",(function():* {
				var onTime:Function;
				return onTime = function(param1:Event):void {
					timer.removeEventListener("timer",onTime);
					func();
				};
			})());
			timer.start();
		}
		
		private function createStarlingChild() : void {
			var _loc1_:Class = null;
			var _loc3_:Object = null;
			var _loc2_:ApplicationDomain = loader.contentLoaderInfo.applicationDomain;
			if(_loc2_.hasDefinition("RymdenRunt")) {
				_loc1_ = _loc2_.getDefinition("RymdenRunt") as Class;
				_loc3_ = {
					"origin":"desktop",
					"version":1
				};
				childSWF = new _loc1_(_loc3_);
			}
		}
		
		private function log(param1:String) : void {
			if(true) {
				return;
			}
			tf.appendText(param1 + "\n");
			tf.scrollV = tf.maxScrollV;
		}
		
		private function onExit(param1:Event = null) : void {
			log("Exiting application, cleaning up");
			if(childSWF) {
				childSWF.removeEventListener("exitgame",onExit);
				removeChild(childSWF);
			}
			NativeApplication.nativeApplication.removeEventListener("exiting",onExit);
			NativeApplication.nativeApplication.exit();
		}
	}
}

