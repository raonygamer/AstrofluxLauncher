package {
	import flash.display.Shape;
	import flash.display.Sprite;
	import flash.events.MouseEvent;
	import flash.text.TextField;
	import flash.text.TextFormat;
	
	public class Popup extends Sprite {
		protected var box:Shape;
		
		protected var yesBtn:Sprite;
		
		protected var tf:TextField;
		
		public function Popup(message:String, callback:Function) {
			var okText:TextField;
			super();
			box = new Shape();
			yesBtn = new Sprite();
			addChild(box);
			addChild(yesBtn);
			tf = new TextField();
			tf.x = 10;
			tf.y = 10;
			tf.text = message;
			tf.autoSize = "left";
			tf.width = 250;
			tf.setTextFormat(new TextFormat("Verdana",18,0xffffff));
			tf.wordWrap = true;
			box.graphics.lineStyle(1);
			box.graphics.beginFill(0,1);
			box.graphics.drawRect(0,0,tf.textWidth + 2 * tf.x,tf.textHeight + 2 * tf.y + 50);
			box.graphics.endFill();
			addChild(box);
			yesBtn.graphics.lineStyle(1,0xff00);
			yesBtn.graphics.beginFill(0xff00,0.4);
			yesBtn.graphics.drawRect(box.width / 2 - 40,tf.textHeight + 30,80,20);
			yesBtn.graphics.endFill();
			yesBtn.useHandCursor = true;
			addChild(yesBtn);
			okText = new TextField();
			okText.x = box.width / 2 - 40;
			okText.y = tf.textHeight + 30;
			okText.text = "OK";
			okText.width = 80;
			okText.height = 20;
			okText.selectable = false;
			okText.setTextFormat(new TextFormat("Verdana",16,0xffffff,null,null,null,null,null,"center"));
			addChild(tf);
			addChild(okText);
			
			var yesClickHandler:Function = function(e:MouseEvent):void {
				okText.removeEventListener("click", yesClickHandler);
				callback();
			}
			
			okText.addEventListener("click", yesClickHandler);
		}
	}
}

