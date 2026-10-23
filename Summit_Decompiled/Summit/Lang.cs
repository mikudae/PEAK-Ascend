using System;
using System.Collections.Generic;

namespace Summit;

internal static class Lang
{
	public static readonly string[] Names = new string[3] { "English", "Español", "中文" };

	private static readonly string[] codes = new string[3] { "en", "es", "zh" };

	internal static int Now;

	internal static readonly HashSet<string> Missing = new HashSet<string>();

	internal static Func<string> OfTheGame = () => (string)null;

	private static Dictionary<string, string[]> table;

	private static Dictionary<string, string[]> Table
	{
		get
		{
			if (table == null)
			{
				table = new Dictionary<string, string[]>();
				Common(table);
				Own(table);
			}
			return table;
		}
	}

	internal static int Count => Table.Count;

	private static void Common(Dictionary<string, string[]> t)
	{
		t["Home"] = new string[2] { "Inicio", "主页" };
		t["About"] = new string[2] { "Acerca de", "关于" };
		t["error: {0}"] = new string[2] { "error: {0}", "错误：{0}" };
		t["This menu"] = new string[2] { "Este menú", "本菜单" };
		t["Settings"] = new string[2] { "Ajustes", "设置" };
		t["Language"] = new string[2] { "Idioma", "语言" };
		t["The language of this menu."] = new string[2] { "El idioma de este menú.", "本菜单的语言。" };
		t["Remember my switches and sliders"] = new string[2] { "Recordar mis interruptores y controles", "记住我的开关和滑块" };
		t["On: every switch and slider is saved as it changes and comes back the next time you start the game. Off: they all start from normal each time."] = new string[2] { "Encendido: cada interruptor y control se guarda al cambiar y vuelve la próxima vez que abras el juego. Apagado: todos empiezan en normal cada vez.", "开启：每个开关和滑块在更改时都会保存，下次启动游戏时恢复。关闭：每次都从默认值开始。" };
		t["Show what is switched on"] = new string[2] { "Mostrar lo que está encendido", "显示已开启的功能" };
		t["While the menu is closed, a line at the top of the screen lists everything that is switched on."] = new string[2] { "Con el menú cerrado, una línea en lo alto de la pantalla enumera todo lo que está encendido.", "菜单关闭时，屏幕顶部会用一行文字列出所有已开启的功能。" };
		t["Block achievements while this mod is loaded"] = new string[2] { "Bloquear logros mientras este mod esté cargado", "加载此模组时屏蔽成就" };
		t["On by default, so nothing you do with the menu ends up on your profile. Your choice is saved."] = new string[2] { "Encendido de serie, para que nada de lo que hagas con el menú acabe en tu perfil. Tu elección se guarda.", "默认开启，这样你用菜单做的任何事都不会记入你的个人资料。你的选择会被保存。" };
		t["Keys and reset"] = new string[2] { "Teclas y restablecer", "按键与重置" };
		t["Menu key"] = new string[2] { "Tecla del menú", "菜单按键" };
		t["The key that opens and closes this menu. Press Change and then the key you want. It is saved."] = new string[2] { "La tecla que abre y cierra este menú. Pulsa Cambiar y luego la tecla que quieras. Se guarda.", "打开和关闭此菜单的按键。点击“更改”，然后按下想要的按键。会自动保存。" };
		t["Switches everything off and puts every slider back, as when the game has just started."] = new string[2] { "Apaga todo y devuelve cada control a su sitio, como al abrir el juego.", "关闭所有开关并将每个滑块复位，就像刚启动游戏时一样。" };
		t["Put everything back to normal"] = new string[2] { "Volver todo a la normalidad", "全部恢复默认" };
		t["Everything is back to normal"] = new string[2] { "Todo ha vuelto a la normalidad", "已全部恢复默认" };
		t["Shortcut keys"] = new string[2] { "Atajos de teclado", "快捷键" };
		t["Give a switch or an action a key and it works without opening the menu. Press its button and then the key. Backspace takes the key away, Escape leaves it as it was."] = new string[2] { "Dale una tecla a un interruptor o a una acción y funcionará sin abrir el menú. Pulsa su botón y luego la tecla. Retroceso quita la tecla, Escape la deja como estaba.", "为开关或操作指定按键后，无需打开菜单即可使用。点击它的按钮，然后按下按键。退格键清除按键，Esc 保持不变。" };
		t["Its key: {0}."] = new string[2] { "Su tecla: {0}.", "快捷键：{0}。" };
		t["Click to take this off the home page."] = new string[2] { "Haz clic para quitarlo de Inicio.", "点击可从主页移除。" };
		t["Click to pin this to the home page."] = new string[2] { "Haz clic para fijarlo en Inicio.", "点击可固定到主页。" };
		t["Pinned to Home"] = new string[2] { "Fijado en Inicio", "已固定到主页" };
		t["Taken off Home"] = new string[2] { "Quitado de Inicio", "已从主页移除" };
		t["Go to {0}: {1}"] = new string[2] { "Ir a {0}: {1}", "前往{0}：{1}" };
		t["Pinned"] = new string[2] { "Fijado", "已固定" };
		t["Nothing is pinned yet. The few things you use most can live here, one click away."] = new string[2] { "Aún no hay nada fijado. Lo que más uses puede estar aquí, a un clic.", "还没有固定任何内容。把最常用的几项放在这里，一点就到。" };
		t["Any switch, slider or row of buttons"] = new string[2] { "Cualquier interruptor, deslizador o fila de botones", "任意开关、滑块或一排按钮" };
		t["On any page, move the mouse over a row. A small square shows at its left edge, like the coloured one above. Click it and the row is pinned here. Click it again to take it off."] = new string[2] { "En cualquier página, pasa el ratón por una fila. En su borde izquierdo aparece un cuadradito, como el de color de arriba. Haz clic y la fila queda fijada aquí. Haz clic otra vez para quitarla.", "在任意页面，将鼠标移到某一行上，其左边缘会出现一个小方块，就像上面那个彩色的。点击它，该行就会固定到这里；再点一次即可取消。" };
		t["{0} pinned"] = new string[2] { "{0} fijados", "已固定 {0} 项" };
		t["Switched on now"] = new string[2] { "Encendido ahora", "当前已开启" };
		t["Nothing is switched on."] = new string[2] { "No hay nada activado.", "目前没有开启任何功能。" };
		t["Search"] = new string[2] { "Buscar", "搜索" };
		t["Nothing in the menu matches \"{0}\"."] = new string[2] { "Nada en el menú coincide con \"{0}\".", "菜单中没有与“{0}”匹配的内容。" };
		t["{0} found"] = new string[2] { "{0} encontrados", "找到 {0} 项" };
		t["Menu key is still {0}"] = new string[2] { "La tecla del menú sigue siendo {0}", "菜单按键仍为 {0}" };
		t["No change"] = new string[2] { "Sin cambios", "未更改" };
		t["{0}: no key"] = new string[2] { "{0}: sin tecla", "{0}：无按键" };
		t["{0} already opens the menu"] = new string[2] { "{0} ya abre el menú", "{0} 已用于打开菜单" };
		t["Menu key is now {0}"] = new string[2] { "La tecla del menú ahora es {0}", "菜单按键已改为 {0}" };
		t["Search every switch, slider and button"] = new string[2] { "Buscar cualquier interruptor, deslizador o botón", "搜索所有开关、滑块和按钮" };
		t["{0} to close"] = new string[2] { "{0} para cerrar", "按 {0} 关闭" };
		t["Move the mouse over anything to read what it does."] = new string[2] { "Pasa el ratón por cualquier cosa para leer qué hace.", "把鼠标移到任意项目上，即可查看它的作用。" };
		t["On Home. The square at its\nleft edge takes it off."] = new string[2] { "En Inicio. El cuadrado de su\nborde izquierdo lo quita.", "已在主页。点左侧的\n小方块可移除。" };
		t["The square at its left edge\npins it to Home."] = new string[2] { "El cuadrado de su borde\nizquierdo lo fija en Inicio.", "点左侧的小方块\n可固定到主页。" };
		t["Made by {0}."] = new string[2] { "Hecho por {0}.", "作者：{0}。" };
		t["Not working in this game version: {0}"] = new string[2] { "No funciona en esta versión del juego: {0}", "在此游戏版本中无法使用：{0}" };
		t["Join the Discord"] = new string[2] { "Únete al Discord", "加入 Discord" };
		t["Press the key you want. Escape cancels."] = new string[2] { "Pulsa la tecla que quieras. Escape cancela.", "按下想用的按键。按 Esc 取消。" };
		t["The menu opens with {0}"] = new string[2] { "El menú se abre con {0}", "菜单按键：{0}" };
		t["Cancel"] = new string[2] { "Cancelar", "取消" };
		t["Change"] = new string[2] { "Cambiar", "更改" };
		t["Press a key"] = new string[2] { "Pulsa una tecla", "请按键" };
		t["No key"] = new string[2] { "Sin tecla", "无按键" };
		t["on"] = new string[2] { "encendido", "开" };
		t["off"] = new string[2] { "apagado", "关" };
		t["Nothing matches"] = new string[2] { "No hay coincidencias", "没有匹配项" };
	}

	private static void Own(Dictionary<string, string[]> t)
	{
		t["the game's"] = new string[2] { "la del juego", "游戏默认" };
		t["That scout is not down"] = new string[2] { "Ese explorador no está caído", "该童子军并未倒下" };
		t["Only the host can clear another scout's statuses"] = new string[2] { "Solo el anfitrión puede quitar los estados de otro explorador", "只有房主才能清除其他童子军的状态" };
		t["Swapping needs one scout picked"] = new string[2] { "Para cambiar de sitio hay que elegir un explorador", "互换位置需要先选中一名童子军" };
		t["Swapped"] = new string[2] { "Cambiados", "已互换" };
		t["These are pranks a scout walks away from: nothing here kills, kicks or takes anything. A hard launch near a ledge can still hurt, so mind where they are standing. Use them with friends who are in on the joke."] = new string[2] { "Son bromas de las que un explorador sale andando: nada aquí mata, expulsa ni quita nada. Un lanzamiento fuerte junto a un borde puede hacer daño, así que mira dónde están. Úsalas con amigos que estén al tanto de la broma.", "这些只是无伤大雅的恶作剧：这里的功能不会致死、踢人或拿走任何东西。但在悬崖边用力抛掷仍可能造成伤害，请留意他们站的位置。请只对知情的朋友使用。" };
		t["Scout"] = new string[2] { "Explorador", "童子军" };
		t["Nobody to prank"] = new string[2] { "No hay a quién gastar la broma", "没有可恶作剧的对象" };
		t["Pick a scout first"] = new string[2] { "Elige primero un explorador", "请先选择一名童子军" };
		t["Everyone"] = new string[2] { "Todos", "所有人" };
		t["Nobody else is here. This page needs a lobby with other scouts in it."] = new string[2] { "No hay nadie más aquí. Esta página necesita una sala con otros exploradores.", "这里没有其他人。此页面需要房间内有其他童子军。" };
		t["Unlock every cosmetic for good"] = new string[2] { "Desbloquear todos los cosméticos para siempre", "永久解锁全部外观" };
		t["Every cosmetic is already yours"] = new string[2] { "Ya tienes todos los cosméticos", "你已拥有全部外观" };
		t["Empty"] = new string[2] { "Vacío", "空" };
		t["Wear"] = new string[2] { "Poner", "穿上" };
		t["Save"] = new string[2] { "Guardar", "保存" };
		t["Remember"] = new string[2] { "Recordar", "记住" };
		t["Go"] = new string[2] { "Ir", "前往" };
		t["Nobody else is here."] = new string[2] { "No hay nadie más aquí.", "这里没有其他人。" };
		t["Pick something on the Items page first."] = new string[2] { "Elige primero algo en la página Objetos.", "请先在“物品”页面选择一件物品。" };
		t["Pick something on the Items page and it can be dropped on their head from here."] = new string[2] { "Elige algo en la página Objetos y desde aquí se les puede dejar caer en la cabeza.", "在“物品”页面选择一件物品后，就可以从这里砸到他们头上。" };
		t["Pick an item"] = new string[2] { "Elige un objeto", "选择一件物品" };
		t["It goes into your hands when you host or play alone. In someone else's lobby, or with full pockets, it lands in front of you."] = new string[2] { "Va a tus manos si eres anfitrión o juegas solo. En la sala de otro, o con los bolsillos llenos, cae delante de ti.", "当你是房主或单人游戏时，它会直接到你手中。在别人的房间里或口袋已满时，它会落在你面前。" };
		t["Spawn"] = new string[2] { "Crear", "生成" };
		t["The Help and Troll pages can hand this item to another scout."] = new string[2] { "Las páginas Ayudar y Bromas pueden dar este objeto a otro explorador.", "“帮助”和“恶作剧”页面可以把这件物品交给其他童子军。" };
		t["Luggage"] = new string[2] { "Equipaje", "行李" };
		t["Campfire"] = new string[2] { "Hoguera", "营火" };
		t["Scout statue"] = new string[2] { "Estatua de explorador", "童子军雕像" };
		t["{0} is back up"] = new string[2] { "{0} vuelve a estar en pie", "{0} 已重新站起来" };
		t["{0}: statuses cleared"] = new string[2] { "{0}: estados quitados", "{0}：状态已清除" };
		t["{0} brought over"] = new string[2] { "{0} traído", "已带来 {0}" };
		t["Click again: {0} Steam achievements and ascent {1}, for good"] = new string[2] { "Clic otra vez: {0} logros de Steam y ascenso {1}, para siempre", "再次点击：永久获得 {0} 个 Steam 成就并解锁进阶 {1}" };
		t["Click again: {0} Steam achievements, for good"] = new string[2] { "Clic otra vez: {0} logros de Steam, para siempre", "再次点击：永久获得 {0} 个 Steam 成就" };
		t["Outfit {0}"] = new string[2] { "Conjunto {0}", "套装 {0}" };
		t["Spot {0}: {1} m away"] = new string[2] { "Punto {0}: a {1} m", "位置 {0}：距离 {1} 米" };
		t["Spot {0}: empty"] = new string[2] { "Punto {0}: vacío", "位置 {0}：空" };
		t["To {0}"] = new string[2] { "A {0}", "传送到 {0}" };
		t["Hand them a {0}"] = new string[2] { "Darle: {0}", "递出：{0}" };
		t["{0} for {1}"] = new string[2] { "{0} para {1}", "{0} 已给 {1}" };
		t["Drop a {0} on their head"] = new string[2] { "Dejar caer en su cabeza: {0}", "砸到头上：{0}" };
		t["{0} incoming"] = new string[2] { "Ahí va: {0}", "{0} 来了" };
		t["Survival"] = new string[2] { "Supervivencia", "生存" };
		t["Right now"] = new string[2] { "Ahora mismo", "立即生效" };
		t["Cosmetics"] = new string[2] { "Cosméticos", "外观" };
		t["Outfits"] = new string[2] { "Conjuntos", "套装" };
		t["Movement"] = new string[2] { "Movimiento", "移动" };
		t["On foot"] = new string[2] { "A pie", "步行" };
		t["Climbing"] = new string[2] { "Escalada", "攀爬" };
		t["Flying"] = new string[2] { "Vuelo", "飞行" };
		t["Teleport"] = new string[2] { "Teletransporte", "传送" };
		t["Places"] = new string[2] { "Lugares", "地点" };
		t["My spots"] = new string[2] { "Mis puntos", "我的位置" };
		t["Scouts"] = new string[2] { "Exploradores", "童子军" };
		t["World"] = new string[2] { "Mundo", "世界" };
		t["See through the mountain"] = new string[2] { "Ver a través de la montaña", "透视山体" };
		t["The mountain"] = new string[2] { "La montaña", "山" };
		t["Fog rise speed"] = new string[2] { "Velocidad de la niebla", "雾气上升速度" };
		t["Fog height level"] = new string[2] { "Nivel de altura de la niebla", "雾气高度调节" };
		t["Lava rise speed"] = new string[2] { "Velocidad de la lava", "熔岩上升速度" };
		t["Lava height level"] = new string[2] { "Nivel de altura de la lava", "熔岩高度调节" };
		t["Time and route"] = new string[2] { "Hora y ruta", "时间与路线" };
		t["Camera"] = new string[2] { "Cámara", "镜头" };
		t["View"] = new string[2] { "Vista", "视角" };
		t["Free camera"] = new string[2] { "Cámara libre", "自由镜头" };
		t["Help"] = new string[2] { "Ayudar", "帮助" };
		t["Who"] = new string[2] { "Quién", "对象" };
		t["Back on their feet"] = new string[2] { "De nuevo en pie", "重新站起来" };
		t["Bring and give"] = new string[2] { "Traer y dar", "带来与给予" };
		t["Troll"] = new string[2] { "Bromas", "恶作剧" };
		t["Shove"] = new string[2] { "Empujón", "推搡" };
		t["Mess with"] = new string[2] { "Fastidiar", "捉弄" };
		t["Fair warning"] = new string[2] { "Aviso", "温馨提示" };
		t["God mode"] = new string[2] { "Modo dios", "无敌模式" };
		t["No injury, hunger, cold, poison or any other status. Things that kill outright, like dropping off the bottom of the map, still do."] = new string[2] { "Sin heridas, hambre, frío, veneno ni ningún otro estado. Lo que mata de golpe, como caer por debajo del mapa, sigue matando.", "不会受伤、饥饿、寒冷、中毒或获得任何其他状态。直接致死的情况（例如掉出地图底部）仍然有效。" };
		t["Infinite stamina"] = new string[2] { "Resistencia infinita", "无限体力" };
		t["Your stamina bar stays full: run, climb and jump as long as you like."] = new string[2] { "Tu barra de resistencia sigue llena: corre, escala y salta cuanto quieras.", "体力条始终保持全满：尽情奔跑、攀爬和跳跃。" };
		t["No fall damage"] = new string[2] { "Sin daño por caída", "无坠落伤害" };
		t["A long drop does not hurt you and does not knock you over."] = new string[2] { "Una caída larga no te hace daño ni te tumba.", "从高处落下不会受伤，也不会摔倒。" };
		t["Nothing weighs anything"] = new string[2] { "Nada pesa", "物品无重量" };
		t["What you carry adds no weight to your stamina bar."] = new string[2] { "Lo que llevas no añade peso a tu barra de resistencia.", "携带的物品不会给体力条增加负重。" };
		t["Items never run out"] = new string[2] { "Los objetos no se gastan", "物品永不耗尽" };
		t["Food, medicine, pitons, rope and anything else with a number of uses is not used up when you use it. Only for what you hold yourself."] = new string[2] { "La comida, las medicinas, los pitones, la cuerda y todo lo que tenga usos no se gasta al usarlo. Solo para lo que llevas tú.", "食物、药品、岩钉、绳索以及其他有使用次数的物品在使用时不会消耗。仅对你自己手持的物品有效。" };
		t["Every cosmetic, for this session"] = new string[2] { "Todos los cosméticos, esta sesión", "本次游戏解锁全部外观" };
		t["Open your passport and pick anything. Nothing is written to Steam, and the locks are back the next time you start the game."] = new string[2] { "Abre tu pasaporte y elige lo que quieras. No se escribe nada en Steam, y los candados vuelven la próxima vez que abras el juego.", "打开护照即可任意选择。不会向 Steam 写入任何内容，下次启动游戏时锁定会恢复。" };
		t["Jump again in mid-air"] = new string[2] { "Saltar otra vez en el aire", "空中连跳" };
		t["Jump as often as you like without touching the ground."] = new string[2] { "Salta cuantas veces quieras sin tocar el suelo.", "无需落地即可随意多次跳跃。" };
		t["Grab any surface"] = new string[2] { "Agarrarse a cualquier superficie", "可抓握任意表面" };
		t["You can start climbing on overhangs and on ground the game calls too flat or too steep to hold."] = new string[2] { "Puedes empezar a escalar en desplomes y en suelo que el juego considera demasiado llano o demasiado empinado.", "你可以在悬垂面以及游戏认为过平或过陡的表面上开始攀爬。" };
		t["Never slip"] = new string[2] { "No resbalar nunca", "永不打滑" };
		t["You do not slide down a wall, on ice, when out of stamina or after a fall onto it."] = new string[2] { "No resbalas por una pared, ni en el hielo, ni sin resistencia, ni tras caer sobre ella.", "无论在冰面、体力耗尽还是摔落到墙上，你都不会从墙面滑落。" };
		t["Fly"] = new string[2] { "Volar", "飞行" };
		t["Move keys fly where you look, jump goes up, crouch goes down, sprint goes faster. Let go and you hover. Fall damage is off while flying."] = new string[2] { "Las teclas de movimiento vuelan hacia donde miras, saltar sube, agacharse baja, correr acelera. Suelta y te quedas flotando. No hay daño por caída mientras vuelas.", "移动键朝视线方向飞行，跳跃上升，蹲下下降，冲刺加速。松开按键则悬停。飞行时没有坠落伤害。" };
		t["Pass through walls while flying"] = new string[2] { "Atravesar paredes al volar", "飞行时穿墙" };
		t["While you fly, nothing stops you: not walls, not the ground."] = new string[2] { "Mientras vuelas nada te detiene: ni las paredes ni el suelo.", "飞行时没有任何东西能挡住你：墙壁和地面都不行。" };
		t["Other scouts"] = new string[2] { "Otros exploradores", "其他童子军" };
		t["Names and distances of the other scouts, drawn over the game, through walls."] = new string[2] { "Nombres y distancias de los otros exploradores, dibujados sobre el juego, a través de las paredes.", "在游戏画面上透过墙壁显示其他童子军的名字和距离。" };
		t["Unopened luggage"] = new string[2] { "Equipaje sin abrir", "未打开的行李" };
		t["Luggage nobody has opened yet, within 250 m."] = new string[2] { "Equipaje que nadie ha abierto aún, a menos de 250 m.", "250 米内尚未被打开的行李。" };
		t["Items on the ground"] = new string[2] { "Objetos en el suelo", "地上的物品" };
		t["Loose items lying around, within 80 m."] = new string[2] { "Objetos sueltos por ahí, a menos de 80 m.", "80 米内散落在地上的物品。" };
		t["Campfires"] = new string[2] { "Hogueras", "营火" };
		t["Every campfire on the mountain."] = new string[2] { "Todas las hogueras de la montaña.", "山上的所有营火。" };
		t["Scout statues"] = new string[2] { "Estatuas de explorador", "童子军雕像" };
		t["The statues that bring fallen scouts back, within 600 m."] = new string[2] { "Las estatuas que devuelven a los exploradores caídos, a menos de 600 m.", "600 米内可以复活倒下童子军的雕像。" };
		t["Hazards"] = new string[2] { "Peligros", "危险物" };
		t["Bees, scorpions, spiders, zombies, tornados and the other things that hurt, within 150 m."] = new string[2] { "Abejas, escorpiones, arañas, zombis, tornados y lo demás que hace daño, a menos de 150 m.", "150 米内的蜜蜂、蝎子、蜘蛛、僵尸、龙卷风等会造成伤害的东西。" };
		t["The fog never comes"] = new string[2] { "La niebla no llega nunca", "迷雾永不来袭" };
		t["The host's fog is the lobby's fog: this holds it for everyone when you host, and does nothing lasting when you do not."] = new string[2] { "La niebla del anfitrión es la de la sala: esto la detiene para todos si eres anfitrión, y no hace nada duradero si no lo eres.", "房主的迷雾就是整个房间的迷雾：你是房主时会为所有人挡住它，不是房主时则没有持续效果。" };
		t["The lava does not rise"] = new string[2] { "La lava no sube", "熔岩不再上涨" };
		t["Holds the rising lava where it is. The host's lava is the lobby's lava."] = new string[2] { "Mantiene la lava donde está. La lava del anfitrión es la de la sala.", "让上涨的熔岩停在原处。房主的熔岩就是整个房间的熔岩。" };
		t["No hazards"] = new string[2] { "Sin peligros", "关闭危险物" };
		t["Switches off every hazard the game lets a run switch off: bees, urchins, scorpions, tornados, thorns and the rest. Switch it on before the run starts: what is already on the mountain may stay. The host decides for the lobby."] = new string[2] { "Apaga todos los peligros que el juego permite apagar en una partida: abejas, erizos, escorpiones, tornados, espinas y el resto. Enciéndelo antes de empezar la partida: lo que ya esté en la montaña puede quedarse. El anfitrión decide por la sala.", "关闭游戏允许在一局中关闭的所有危险物：蜜蜂、海胆、蝎子、龙卷风、荆棘等。请在开局前开启：已经出现在山上的可能会保留。由房主为整个房间决定。" };
		t["No rain, wind, storm, heat or cold nights"] = new string[2] { "Sin lluvia, viento, tormenta, calor ni noches frías", "无雨、风、暴风、高温和寒夜" };
		t["The weather stays out of your way. Switch it on before the run starts. The host decides for the lobby."] = new string[2] { "El tiempo no te molesta. Enciéndelo antes de empezar la partida. El anfitrión decide por la sala.", "天气不再妨碍你。请在开局前开启。由房主为整个房间决定。" };
		t["Third person"] = new string[2] { "Tercera persona", "第三人称" };
		t["See your scout from behind. The camera stops short of walls."] = new string[2] { "Ve a tu explorador desde atrás. La cámara se detiene antes de las paredes.", "从背后观察你的童子军。镜头会在碰到墙壁前停下。" };
		t["Hide the game's HUD"] = new string[2] { "Ocultar el HUD del juego", "隐藏游戏界面" };
		t["Takes the stamina bar, the item slots and the prompts off the screen, for screenshots."] = new string[2] { "Quita de la pantalla la barra de resistencia, los huecos de objetos y los avisos, para capturas.", "从屏幕上移除体力条、物品栏和提示，方便截图。" };
		t["Your scout stays where it is and the view flies: move keys, jump for up, crouch for down, sprint for faster. Close the menu to steer. Good for screenshots and for looking ahead."] = new string[2] { "Tu explorador se queda donde está y la vista vuela: teclas de movimiento, saltar para subir, agacharse para bajar, correr para ir más rápido. Cierra el menú para manejarla. Buena para capturas y para mirar más adelante.", "童子军留在原地，视角自由飞行：使用移动键，跳跃上升，蹲下下降，冲刺加速。关闭菜单后即可操控。适合截图和探路。" };
		t["Walk and run speed"] = new string[2] { "Velocidad al andar y correr", "行走与奔跑速度" };
		t["How fast you move on the ground."] = new string[2] { "Lo rápido que te mueves por el suelo.", "你在地面上的移动速度。" };
		t["Jump height"] = new string[2] { "Altura de salto", "跳跃高度" };
		t["How high a jump takes you."] = new string[2] { "Lo alto que llegas con un salto.", "一次跳跃能达到的高度。" };
		t["Climbing speed"] = new string[2] { "Velocidad de escalada", "攀爬速度" };
		t["How fast you move on a wall."] = new string[2] { "Lo rápido que te mueves por una pared.", "你在墙面上的移动速度。" };
		t["Reach"] = new string[2] { "Alcance", "抓握距离" };
		t["How far away a wall can be for you to grab it."] = new string[2] { "A qué distancia puede estar una pared para que la agarres.", "墙面距离多远时你仍能抓住它。" };
		t["Rope length"] = new string[2] { "Longitud de la cuerda", "绳索长度" };
		t["How long a rope from a spool can get. The spool is kept full enough for it."] = new string[2] { "Lo larga que puede ser la cuerda de un carrete. El carrete se mantiene lo bastante lleno.", "绳索卷轴放出的绳子能有多长。卷轴会保持足够的余量。" };
		t["Flying speed"] = new string[2] { "Velocidad de vuelo", "飞行速度" };
		t["How fast you fly, in metres per second. Sprint is two and a half times this."] = new string[2] { "Lo rápido que vuelas, en metros por segundo. Corriendo es dos veces y media más.", "飞行速度，单位为米/秒。冲刺时为其 2.5 倍。" };
		t["Distance"] = new string[2] { "Distancia", "距离" };
		t["How far behind your scout the third-person camera sits."] = new string[2] { "A qué distancia por detrás de tu explorador está la cámara en tercera persona.", "第三人称镜头位于童子军身后多远。" };
		t[" m"] = new string[2] { " m", " 米" };
		t["Field of view"] = new string[2] { "Campo de visión", "视野" };
		t["How wide the view is. All the way left leaves it to the game's own setting."] = new string[2] { "Lo ancha que es la vista. Del todo a la izquierda se deja al ajuste del propio juego.", "视野的宽度。拖到最左侧则使用游戏自身的设置。" };
		t["Clears every status and fills your stamina. Bonus stamina gives you the extra yellow bar."] = new string[2] { "Quita todos los estados y llena tu resistencia. La resistencia extra te da la barra amarilla adicional.", "清除所有状态并回满体力。“额外体力”会给你额外的黄色体力条。" };
		t["Clear statuses"] = new string[2] { "Quitar estados", "清除状态" };
		t["Bonus stamina"] = new string[2] { "Resistencia extra", "额外体力" };
		t["Brings you back where you went down. The game drops what you were carrying when it revives a scout."] = new string[2] { "Te devuelve donde caíste. El juego suelta lo que llevabas al revivir a un explorador.", "在你倒下的位置复活。游戏在复活童子军时会掉落其携带的物品。" };
		t["Revive me"] = new string[2] { "Revivirme", "复活自己" };
		t["Instantly kills your scout."] = new string[2] { "Mata instantáneamente a tu explorador.", "立即击杀自己的童子军。" };
		t["Kill self"] = new string[2] { "Suicidarse", "击杀自己" };
		t["You are already dead"] = new string[2] { "Ya estás muerto", "你已经阵亡了" };
		t["Killed self"] = new string[2] { "Te has suicidado", "已自杀" };
		t["Aim first, then open the menu and press the button. It also has a shortcut key on the About page."] = new string[2] { "Apunta primero, luego abre el menú y pulsa el botón. También tiene un atajo de teclado en la página Acerca de.", "先瞄准，再打开菜单并点击按钮。也可以在“关于”页面为它设置快捷键。" };
		t["To where I am looking"] = new string[2] { "A donde estoy mirando", "传送到视线所指处" };
		t["The campfire and the scout statue of the part of the mountain you are on."] = new string[2] { "La hoguera y la estatua de explorador de la parte de la montaña en la que estás.", "你所在山段的营火和童子军雕像。" };
		t["To the campfire"] = new string[2] { "A la hoguera", "传送到营火" };
		t["To the scout statue"] = new string[2] { "A la estatua de explorador", "传送到童子军雕像" };
		t["Sets the time of day. Away from the host, the host's clock takes over again."] = new string[2] { "Pone la hora del día. Si no eres el anfitrión, su reloj vuelve a mandar.", "设置一天中的时间。如果你不是房主，房主的时钟会重新接管。" };
		t["dawn"] = new string[2] { "el amanecer", "黎明" };
		t["noon"] = new string[2] { "el mediodía", "正午" };
		t["dusk"] = new string[2] { "el anochecer", "黄昏" };
		t["midnight"] = new string[2] { "la medianoche", "午夜" };
		t["Dawn"] = new string[2] { "Amanecer", "黎明" };
		t["Noon"] = new string[2] { "Mediodía", "正午" };
		t["Dusk"] = new string[2] { "Anochecer", "黄昏" };
		t["Midnight"] = new string[2] { "Medianoche", "午夜" };
		t["Moves the whole lobby on to the next part of the mountain. Host only."] = new string[2] { "Lleva a toda la sala a la siguiente parte de la montaña. Solo el anfitrión.", "将整个房间推进到山的下一段。仅限房主。" };
		t["Skip to the next section"] = new string[2] { "Saltar a la siguiente sección", "跳到下一段" };
		t["Revives a scout who is down: where they fell, or right next to you."] = new string[2] { "Revive a un explorador caído: donde cayó, o justo a tu lado.", "复活倒下的童子军：在其倒下的位置，或者就在你身边。" };
		t["Revive where they fell"] = new string[2] { "Revivir donde cayó", "在倒下处复活" };
		t["Revive next to me"] = new string[2] { "Revivir a mi lado", "在我身边复活" };
		t["Clears a scout's statuses. The game only takes this from the host."] = new string[2] { "Quita los estados de un explorador. El juego solo lo acepta del anfitrión.", "清除一名童子军的状态。游戏只接受房主的此项操作。" };
		t["Clear their statuses"] = new string[2] { "Quitar sus estados", "清除其状态" };
		t["Brings the scout to you, or takes you to them."] = new string[2] { "Trae al explorador hasta ti, o te lleva hasta él.", "把童子军带到你身边，或者把你送到其身边。" };
		t["Bring to me"] = new string[2] { "Traer hasta mí", "带到我身边" };
		t["Go to them"] = new string[2] { "Ir hasta él", "前往其身边" };
		t["Launches them up, throws them the way you look, or pulls them to you."] = new string[2] { "Los lanza hacia arriba, los tira hacia donde miras, o los atrae hacia ti.", "把他们抛向空中、朝你视线方向扔出，或拉向你。" };
		t["Up they go"] = new string[2] { "Allá van", "飞上天了" };
		t["Thrown"] = new string[2] { "Lanzados", "已扔出" };
		t["Pulled"] = new string[2] { "Atraídos", "已拉过来" };
		t["Launch upward"] = new string[2] { "Lanzar hacia arriba", "向上抛" };
		t["Throw where I look"] = new string[2] { "Tirar hacia donde miro", "朝视线方向扔" };
		t["Pull toward me"] = new string[2] { "Atraer hacia mí", "拉向我" };
		t["Knocks them over for three seconds, or makes them hop."] = new string[2] { "Los tumba durante tres segundos, o los hace dar un salto.", "让他们摔倒三秒，或者让他们跳一下。" };
		t["Tripped"] = new string[2] { "Zancadilla hecha", "已绊倒" };
		t["Hop"] = new string[2] { "Salto", "跳了一下" };
		t["Trip (3 seconds)"] = new string[2] { "Zancadilla (3 segundos)", "绊倒（3 秒）" };
		t["Make them jump"] = new string[2] { "Hacerlos saltar", "让他们跳" };
		t["Brings them to you, or swaps places with the one scout picked."] = new string[2] { "Los trae hasta ti, o cambia de sitio con el explorador elegido.", "把他们带到你身边，或与选中的那名童子军互换位置。" };
		t["Brought over"] = new string[2] { "Traídos", "已带来" };
		t["Swap places"] = new string[2] { "Cambiar de sitio", "互换位置" };
		t["The game decides what you own by asking Steam for your achievements. Keeping cosmetics means those achievements really go on your Steam profile, and the ascent levels they need are opened too. There is no taking it back."] = new string[2] { "El juego decide lo que tienes preguntando a Steam por tus logros. Conservar los cosméticos significa que esos logros van de verdad a tu perfil de Steam, y que también se abren los niveles de ascenso que piden. No hay vuelta atrás.", "游戏通过向 Steam 查询成就来判断你拥有哪些外观。永久保留外观意味着这些成就会真正记入你的 Steam 个人资料，所需的进阶等级也会一并解锁。此操作无法撤销。" };
		t["Saved outfits"] = new string[2] { "Conjuntos guardados", "已保存的套装" };
		t["Four places to keep an outfit. Save what you are wearing now, and put it back on with one click. Works in the airport and on the mountain."] = new string[2] { "Cuatro huecos para guardar un conjunto. Guarda lo que llevas ahora y vuelve a ponértelo con un clic. Funciona en el aeropuerto y en la montaña.", "四个套装栏位。保存你当前的穿着，之后一键换回。在机场和山上都能使用。" };
		t["Three spots to remember"] = new string[2] { "Tres puntos para recordar", "三个可记忆的位置" };
		t["Remember where you stand and come back to it later. Spots last until you close the game, and only make sense on the mountain they were set on."] = new string[2] { "Recuerda dónde estás y vuelve más tarde. Los puntos duran hasta que cierras el juego, y solo sirven en la montaña donde se marcaron.", "记住你当前的位置，之后可以回来。位置会保留到关闭游戏为止，并且只在设置它们的那座山上有意义。" };
		t["To another scout"] = new string[2] { "A otro explorador", "传送到其他童子军" };
		t["Takes you next to another scout in the lobby."] = new string[2] { "Te lleva junto a otro explorador de la sala.", "将你传送到房间内另一名童子军身边。" };
		t["The scout to help"] = new string[2] { "El explorador al que ayudar", "要帮助的童子军" };
		t["Pick the scout the buttons below are for."] = new string[2] { "Elige el explorador para los botones de abajo.", "选择下方按钮所针对的童子军。" };
		t["Hand them an item"] = new string[2] { "Darle un objeto", "递给其一件物品" };
		t["Puts the item picked on the Items page right in front of the scout."] = new string[2] { "Pone el objeto elegido en la página Objetos justo delante del explorador.", "把在“物品”页面选中的物品放到该童子军面前。" };
		t["Who to prank"] = new string[2] { "A quién gastar la broma", "恶作剧对象" };
		t["Pick one scout, or everyone."] = new string[2] { "Elige un explorador, o todos.", "选择一名童子军，或所有人。" };
		t["Delivery"] = new string[2] { "Entrega", "空投" };
		t["Drops the item picked on the Items page on their head."] = new string[2] { "Deja caer sobre su cabeza el objeto elegido en la página Objetos.", "把在“物品”页面选中的物品砸到他们头上。" };
		t["About these"] = new string[2] { "Sobre estas bromas", "关于这些功能" };
		t["These are pranks a scout walks away from: nothing here kills, kicks or takes anything. A hard launch near a ledge can still hurt, so mind where they are standing."] = new string[2] { "Son bromas de las que un explorador sale andando: nada aquí mata, expulsa ni quita nada. Un lanzamiento fuerte junto a un borde puede hacer daño, así que mira dónde están.", "这些只是无伤大雅的恶作剧：这里的功能不会致死、踢人或拿走任何东西。但在悬崖边用力抛掷仍可能造成伤害，请留意他们站的位置。" };
		t["Ascent level"] = new string[2] { "Nivel de ascenso", "进阶等级" };
		t["The difficulty level of the run: hunger, fall damage, weight and the rest follow it from the moment you move it. 0 is the standard climb."] = new string[2] { "El nivel de dificultad de la partida: el hambre, el daño por caída, el peso y lo demás lo siguen desde que lo mueves. 0 es la escalada normal.", "本局的难度等级：饥饿、坠落伤害、负重等从你调整的那一刻起随之变化。0 为标准攀登。" };
		t["How hard"] = new string[2] { "Con cuánta fuerza", "力度" };
		t["How fast a shoved scout leaves, in metres per second."] = new string[2] { "A qué velocidad sale despedido un explorador empujado, en metros por segundo.", "被推开的童子军飞出去的速度，单位为米/秒。" };
		t["Home"] = new string[2] { "Inicio", "主页" };
		t["Items"] = new string[2] { "Objetos", "物品" };
		t["About"] = new string[2] { "Acerca de", "关于" };
		t["Search every item in the game"] = new string[2] { "Busca cualquier objeto del juego", "搜索游戏中的所有物品" };
		t["Statuses cleared, stamina full"] = new string[2] { "Estados quitados, resistencia llena", "状态已清除，体力已回满" };
		t["Bonus stamina bar filled"] = new string[2] { "Barra de resistencia extra llena", "额外体力条已补满" };
		t["You are not down"] = new string[2] { "No estás caído", "你并未倒下" };
		t["Revived where you went down"] = new string[2] { "Revivido donde caíste", "已在倒下的位置复活" };
		t["where you were looking"] = new string[2] { "donde mirabas", "视线所指处" };
		t["Nothing in sight to land on"] = new string[2] { "No hay nada a la vista donde aterrizar", "视线内没有可落脚的地方" };
		t["No campfire on this part of the map"] = new string[2] { "No hay hoguera en esta parte del mapa", "地图的这一部分没有营火" };
		t["the campfire"] = new string[2] { "la hoguera", "营火" };
		t["No scout statue on this part of the map"] = new string[2] { "No hay estatua de explorador en esta parte del mapa", "地图的这一部分没有童子军雕像" };
		t["the scout statue"] = new string[2] { "la estatua de explorador", "童子军雕像" };
		t["Free camera: on. Move keys fly the view, jump goes up, crouch goes down, sprint goes faster."] = new string[2] { "Cámara libre: encendida. Las teclas de movimiento mueven la vista, saltar sube, agacharse baja, correr acelera.", "自由镜头：开。移动键控制视角飞行，跳跃上升，蹲下下降，冲刺加速。" };
		t["Free camera: off"] = new string[2] { "Cámara libre: apagada", "自由镜头：关" };
		t["This run's settings block achievements - do it from the airport"] = new string[2] { "Los ajustes de esta partida bloquean los logros: hazlo desde el aeropuerto", "本局设置屏蔽了成就，请在机场操作" };
		t["Every cosmetic is unlocked for good"] = new string[2] { "Todos los cosméticos están desbloqueados para siempre", "已永久解锁全部外观" };
		t["No day and night here"] = new string[2] { "Aquí no hay día ni noche", "这里没有昼夜" };
		t["Only the host can move the lobby on"] = new string[2] { "Solo el anfitrión puede hacer avanzar a la sala", "只有房主才能推进房间进度" };
		t["Start a run first"] = new string[2] { "Empieza primero una partida", "请先开始一局" };
		t["Skipped to the next section"] = new string[2] { "Saltado a la siguiente sección", "已跳到下一段" };
		t["Tornado"] = new string[2] { "Tornado", "龙卷风" };
		t["Bees"] = new string[2] { "Abejas", "蜜蜂" };
		t["Antlion"] = new string[2] { "Hormiga león", "蚁狮" };
		t["Zombie"] = new string[2] { "Zombi", "僵尸" };
		t["Spider"] = new string[2] { "Araña", "蜘蛛" };
		t["Beetle"] = new string[2] { "Escarabajo", "甲虫" };
		t["Frog tongue"] = new string[2] { "Lengua de rana", "青蛙舌头" };
		t["Scorpion"] = new string[2] { "Escorpión", "蝎子" };
		t["Teleported to {0}"] = new string[2] { "Teletransportado a {0}", "已传送到 {0}" };
		t["Spot {0} remembered"] = new string[2] { "Punto {0} recordado", "已记住位置 {0}" };
		t["Spot {0} is empty: remember a place first"] = new string[2] { "El punto {0} está vacío: recuerda antes un lugar", "位置 {0} 是空的：请先记住一个地点" };
		t["spot {0}"] = new string[2] { "el punto {0}", "位置 {0}" };
		t["Outfit {0} saved"] = new string[2] { "Conjunto {0} guardado", "套装 {0} 已保存" };
		t["Outfit {0} is empty: save what you wear first"] = new string[2] { "El conjunto {0} está vacío: guarda primero lo que llevas", "套装 {0} 是空的：请先保存当前穿着" };
		t["Wearing outfit {0}"] = new string[2] { "Llevas el conjunto {0}", "已穿上套装 {0}" };
		t["{0} unlocked, {1} would not open"] = new string[2] { "{0} desbloqueados, {1} no se abrieron", "已解锁 {0} 个，{1} 个无法解锁" };
		t["Spawned {0}"] = new string[2] { "Creado: {0}", "已生成 {0}" };
		t["It is now {0}"] = new string[2] { "Ahora es {0}", "现在是{0}" };
		t["It is now {0} (until the host's clock overrides it)"] = new string[2] { "Ahora es {0} (hasta que mande el reloj del anfitrión)", "现在是{0}（直到被房主的时钟覆盖）" };
		t["Teleport to where I am looking"] = new string[2] { "Teletransportarme a donde miro", "传送到视线所指处" };
		t["Teleport to the campfire"] = new string[2] { "Teletransportarme a la hoguera", "传送到营火" };
		t["Clear statuses and fill stamina"] = new string[2] { "Quitar estados y llenar resistencia", "清除状态并回满体力" };
		t["Free camera on and off"] = new string[2] { "Cámara libre: encender y apagar", "开关自由镜头" };
		t["Remember this spot (spot 1)"] = new string[2] { "Recordar este punto (punto 1)", "记住当前位置（位置 1）" };
		t["Go back to spot 1"] = new string[2] { "Volver al punto 1", "回到位置 1" };
		t["Get into the airport or a run first: nothing here works from the title screen."] = new string[2] { "Entra primero en el aeropuerto o en una partida: nada de esto funciona desde la pantalla de título.", "请先进入机场或开始一局：标题画面下这里的功能都无法使用。" };
		t["A cheat menu for PEAK."] = new string[2] { "Un menú de trucos para PEAK.", "PEAK 的作弊菜单。" };
	}

	public static string T(string english)
	{
		if (Now == 0 || string.IsNullOrWhiteSpace(english))
		{
			return english;
		}
		if (Table.TryGetValue(english, out var value) && value[Now - 1] != null)
		{
			return value[Now - 1];
		}
		for (int i = 0; i < english.Length; i++)
		{
			if (char.IsLetter(english[i]))
			{
				Missing.Add(english);
				break;
			}
		}
		return english;
	}

	public static string K(string english)
	{
		return english;
	}

	public static string F(string english, params object[] args)
	{
		return string.Format(T(english), args);
	}

	public static string Every(string english)
	{
		if (string.IsNullOrEmpty(english) || !Table.TryGetValue(english, out var value))
		{
			return english ?? "";
		}
		return english + " " + value[0] + " " + value[1];
	}

	internal static void Know(string name)
	{
		if (!Table.ContainsKey(name))
		{
			Table[name] = new string[2] { name, name };
		}
	}

	internal static bool Has(string english)
	{
		if (Table.TryGetValue(english, out var value) && !string.IsNullOrEmpty(value[0]))
		{
			return !string.IsNullOrEmpty(value[1]);
		}
		return false;
	}

	private static int Game()
	{
		string text = null;
		try
		{
			text = OfTheGame();
		}
		catch
		{
		}
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		text = text.ToLowerInvariant();
		if (text.StartsWith("es") || text.Contains("spanish") || text.Contains("español") || text.Contains("espanol"))
		{
			return 1;
		}
		if (text.StartsWith("zh") || text.Contains("chinese") || text.Contains("中文"))
		{
			return 2;
		}
		return 0;
	}

	public static void Start()
	{
		if (Settings.Paused)
		{
			Now = 0;
			Theme.Refont();
		}
		else
		{
			int num = Array.IndexOf(codes, (Kit.Language.Value ?? "").Trim().ToLowerInvariant());
			Now = ((num >= 0) ? num : Game());
			Theme.Refont();
		}
	}

	public static void Choose(int language)
	{
		Now = Math.Max(0, Math.Min(codes.Length - 1, language));
		if (!Settings.Paused)
		{
			Kit.Language.Value = codes[Now];
		}
		Theme.Refont();
	}
}
