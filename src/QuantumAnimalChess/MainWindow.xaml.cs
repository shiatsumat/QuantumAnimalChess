using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuantumAnimalChess
{
	public enum KomaState { MochiA, MochiB, OnA, OnB }

	public class Koma
	{
		public bool canbeH, canbeK, canbeZ, canbeL, only, isNari;
		public int index;
		public int X, Y;
		public KomaState state;
		public Game game;

		public Koma(int X, int Y, KomaState state, int index, Game game)
		{
			this.X = X; this.Y = Y; this.state = state; this.index = index; this.game = game;
			canbeH = canbeK = canbeZ = canbeL = true;
			only = isNari = false;
		}
		public bool GetCanBe(int g)
		{
			switch (g)
			{
				case 0:
					return canbeH;
				case 1:
					return canbeK;
				case 2:
					return canbeZ;
				case 3:
					return canbeL;
				default:
					throw new Exception("CanBe");
			}
		}
		public void SetCanBe(int g, bool b)
		{
			switch (g)
			{
				case 0:
					canbeH = b;
					break;
				case 1:
					canbeK = b;
					break;
				case 2:
					canbeZ = b;
					break;
				case 3:
					canbeL = b;
					break;
				default:
					throw new Exception("SetCanBe");
			}
		}
		public void OnlyCanBe(int i)
		{
			canbeH = canbeK = canbeZ = canbeL = false;
			only = true;
			SetCanBe(i, true);
		}
		public bool CanMove(int x, int y)
		{
			var k = game.onboards[x, y];
			if (k != null && (state == KomaState.OnA && k.state == KomaState.OnA || state == KomaState.OnB && k.state == KomaState.OnB))
			{
				return false;
			}

			var game2 = game.Clone();
			game2.komas[index].Move(x, y);
			if (!game2.Preexamine()) return false;
			else return true;
		}
		public void Move(int x, int y)
		{
			var k = game.onboards[x, y];
			if (k != null) k.Toru();

			int up = state == KomaState.OnA ? Y - 1 : Y + 1;
			int down = state == KomaState.OnA ? Y + 1 : Y - 1;
			if ((x == X - 1 || x == X + 1) && y == up)
			{
				canbeK = false;
				if (!isNari) canbeH = false;
			}
			else if ((x == X - 1 || x == X + 1) && y == down)
			{
				canbeH = canbeK = false;
				isNari = false;
			}
			else if (x == X && y == up)
			{
				canbeZ = false;
			}
			else if ((x == X && y == down) || ((x == X - 1 || x == X + 1) && y == Y))
			{
				canbeZ = false;
				if (!isNari) canbeH = false;
			}
			else
			{
				canbeH = canbeK = canbeZ = canbeL = false;
			}

			if ((state == KomaState.OnA && y == 0 || state == KomaState.OnB && y == 3) && canbeH == true)
			{
				isNari = true;
			}

			game.onboards[X, Y] = null;
			game.onboards[x, y] = this;
			X = x; Y = y;
		}
		public void Toru()
		{
			switch (state)
			{
				case KomaState.OnA:
					state = KomaState.MochiB;
					game.mochibs.Add(this);
					break;
				case KomaState.OnB:
					state = KomaState.MochiA;
					game.mochias.Add(this);
					break;
			}
			game.onboards[X, Y] = null;
			X = Y = -1;

			if (canbeL)
			{
				if (only) { game.winner = game.gamestate; game.gamestate = GameState.over; }
				else canbeL = false;
			}
		}
		public bool CanUtsu(int x, int y)
		{
			return game.onboards[x, y] == null;
		}
		public void Utsu(int x, int y)
		{
			if (!CanUtsu(x, y)) throw new Exception("error");

			switch (state)
			{
				case KomaState.MochiA:
					game.mochias.Remove(this);
					state = KomaState.OnA;
					break;
				case KomaState.MochiB:
					game.mochibs.Remove(this);
					state = KomaState.OnB;
					break;
				default:
					throw new Exception("error");
			}
			if ((state == KomaState.OnA && y == 0 || state == KomaState.OnB && y == 3) && canbeH == true)
			{
				isNari = true;
			}
			game.onboards[x, y] = this;
			X = x; Y = y;
		}
		public virtual Canvas Draw() { throw new NotImplementedException(); }
		public Koma Clone(Game game)
		{
			Koma k = (Koma)MemberwiseClone();
			k.game = game;
			return k;
		}
	}

	public class GKoma : Koma
	{
		public double wakuwidth = GGame.wakuwidth, wakuheight = GGame.wakuheight, komawidth = GGame.komawidth, komaheight = GGame.komaheight;
		public Image imgH, imgK, imgZ, imgL, imgN, imgH2, imgK2, imgZ2, imgL2, imgN2;
		public bool dragged = false;
		public Canvas square = null;
		public GGame ggame;

		public GKoma(int X, int Y, KomaState state, int index, GGame ggame)
			: base(X, Y, state, index, ggame)
		{
			this.ggame = ggame;

			imgH = GetImage("hina.png", 1);
			imgK = GetImage("kirin.png", 1);
			imgZ = GetImage("zo.png", 1);
			imgL = GetImage("lion.png", 1);
			imgN = GetImage("niwatori.png", 1);

			imgH2 = GetImage("hina.png", 0.3);
			imgK2 = GetImage("kirin.png", 0.3);
			imgZ2 = GetImage("zo.png", 0.3);
			imgL2 = GetImage("lion.png", 0.3);
			imgN2 = GetImage("niwatori.png", 0.3);
		}
		private Image GetImage(string path, double opacity)
		{
			var image = new Image();
			image.Stretch = Stretch.Fill;
			image.Source = new BitmapImage(new Uri(path, UriKind.Relative));
			image.Width = komawidth; image.Height = komaheight; image.Opacity = opacity;
			return image;
		}
		public override Canvas Draw()
		{
			if (square != null) square.Children.Clear();
			square = new Canvas();
			switch (state)
			{
				case KomaState.MochiB:
				case KomaState.OnB:
					square.RenderTransform = new RotateTransform(180, komawidth / 2, komaheight / 2);
					break;
			}
			square.Width = komawidth; square.Height = komaheight;

			square.MouseDown += MouseDown;
			square.MouseUp += MouseUp;

			var tip = new ToolTip();
			square.ToolTip = tip;
			string text = "";
			switch (state)
			{
				case KomaState.OnA:
					text += "先 ";
					break;
				case KomaState.OnB:
					text += "後 ";
					break;
			}

			if (canbeH & !isNari)
			{
				square.Children.Add(only ? imgH : imgH2);
				text += "ひ";
			}
			if (canbeH & isNari)
			{
				square.Children.Add(only ? imgN : imgN2);
				text += "に";
			}
			if (canbeK)
			{
				square.Children.Add(only ? imgK : imgK2);
				text += "き";
			}
			if (canbeZ)
			{
				square.Children.Add(only ? imgZ : imgZ2);
				text += "ぞ";
			}
			if (canbeL)
			{
				square.Children.Add(only ? imgL : imgL2);
				text += "ラ";
			}

			tip.Content = text;

			return square;
		}
		private void MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (game.gamestate == GameState.over) return;
			switch (state)
			{
				case KomaState.OnA:
				case KomaState.MochiA:
					dragged = game.gamestate == GameState.turnA;
					break;
				case KomaState.OnB:
				case KomaState.MochiB:
					dragged = game.gamestate == GameState.turnB;
					break;
			}
			if (dragged)
			{
				switch (state)
				{
					case KomaState.OnA:
					case KomaState.OnB:
						ggame.board.Children.Remove(square);
						break;
					case KomaState.MochiA:
						ggame.mochiA.Children.Remove(square);
						break;
					case KomaState.MochiB:
						ggame.mochiB.Children.Remove(square);
						break;
				}
				ggame.large.Children.Add(square);
				Canvas.SetZIndex(square, 100);
				var p = e.GetPosition(ggame.large);
				Canvas.SetLeft(square, p.X - komawidth / 2);
				Canvas.SetTop(square, p.Y - komaheight / 2);
				ggame.active = this;

				List<Tuple<int, int>> cango = new List<Tuple<int, int>>();
				switch (state)
				{
					case KomaState.MochiA:
					case KomaState.MochiB:
						for (int x = 0; x < 3; x++) for (int y = 0; y < 4; y++) if (CanUtsu(x, y)) cango.Add(Tuple.Create(x, y));
						break;
					case KomaState.OnA:
					case KomaState.OnB:
						for (int x = 0; x < 3; x++) for (int y = 0; y < 4; y++) if (CanMove(x, y)) cango.Add(Tuple.Create(x, y));
						break;
				}
				int n = cango.Count;
				for (int i = 0; i < n; i++)
				{
					var active = new Rectangle();
					active.Width = komawidth; active.Height = komaheight;
					active.Fill = Brushes.Yellow;
					ggame.board.Children.Add(active);
					Canvas.SetLeft(active, wakuwidth * cango[i].Item1 + (wakuwidth - komawidth) / 2);
					Canvas.SetTop(active, wakuheight * cango[i].Item2 + (wakuheight - komaheight) / 2);
				}
			}
		}
		private void MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!dragged) return;
			dragged = false;
			ggame.active = null;
			ggame.large.Children.Remove(square);
			Canvas.SetZIndex(square, 1);
			var p = e.GetPosition(ggame.board);
			if (p.X < 0 || p.X > wakuwidth * 3 || p.Y < 0 || p.Y > wakuheight * 4)
			{
				game.Draw();
				return;
			}
			int x = (int)Math.Floor(p.X / wakuwidth), y = (int)Math.Floor(p.Y / wakuheight);
			if (x < 0 || x > 4 || y < 0 || y > 4) { game.Draw(); return; }
			switch (state)
			{
				case KomaState.OnA:
				case KomaState.OnB:
					if (CanMove(x, y))
					{
						Move(x, y);
						game.Next();
					}
					else
					{
						game.Draw();
					}
					break;
				case KomaState.MochiA:
				case KomaState.MochiB:
					if (CanUtsu(x, y))
					{
						Utsu(x, y);
						game.Next();
					}
					else
					{
						game.Draw();
					}
					break;
			}
		}
	}

	public enum TeState { Move, Utsu }

	public struct Te
	{
		public TeState state;
		public int k;
		public int X, Y;
		public bool IsValid(Game game)
		{
			var koma = game.komas[k];
			if (game.gamestate == GameState.over) throw new Exception("already over");
			switch (koma.state)
			{
				case KomaState.MochiA:
					if (game.gamestate != GameState.turnA || state != TeState.Utsu) return false;
					break;
				case KomaState.OnA:
					if (game.gamestate != GameState.turnA || state != TeState.Move) return false;
					break;
				case KomaState.MochiB:
					if (game.gamestate != GameState.turnB || state != TeState.Utsu) return false;
					break;
				case KomaState.OnB:
					if (game.gamestate != GameState.turnB || state != TeState.Move) return false;
					break;
			}
			switch (state)
			{
				case TeState.Move:
					return koma.CanMove(X, Y);
				case TeState.Utsu:
					return koma.CanUtsu(X, Y);
				default:
					throw new Exception("error");
			}
		}
		public void Go(Game game)
		{
			var koma = game.komas[k];
			switch (state)
			{
				case TeState.Move:
					koma.Move(X, Y);
					break;
				case TeState.Utsu:
					koma.Utsu(X, Y);
					break;
			}
			game.Next();
		}
	}

	public enum GameState { turnA, turnB, over }

	public class Game
	{
		public GameState gamestate, winner;
		public Koma[,] onboards;
		public Koma[] motoas, motobs, komas;
		public List<Koma> mochias, mochibs;

		public const int big = 1000000;

		public Game()
		{
			onboards = new Koma[3, 4];
			motoas = new Koma[4]; motobs = new Koma[4]; komas = new Koma[8];
			mochias = new List<Koma>(); mochibs = new List<Koma>();
		}
		public Game(bool dummy) { }
		public virtual Koma NewKoma(int X, int Y, KomaState state, int index)
		{
			return new Koma(X, Y, state, index, this);
		}
		public void NewGame()
		{
			gamestate = GameState.turnA;
			winner = GameState.over;

			mochias.Clear(); mochibs.Clear();
			for (int x = 0; x < 3; x++) for (int y = 0; y < 4; y++) onboards[x, y] = null;

			onboards[0, 0] = NewKoma(0, 0, KomaState.OnB, 4);
			onboards[1, 0] = NewKoma(1, 0, KomaState.OnB, 5);
			onboards[2, 0] = NewKoma(2, 0, KomaState.OnB, 6);
			onboards[1, 1] = NewKoma(1, 1, KomaState.OnB, 7);
			motobs[0] = onboards[0, 0];
			motobs[1] = onboards[1, 0];
			motobs[2] = onboards[2, 0];
			motobs[3] = onboards[1, 1];
			onboards[1, 2] = NewKoma(1, 2, KomaState.OnA, 0);
			onboards[0, 3] = NewKoma(0, 3, KomaState.OnA, 1);
			onboards[1, 3] = NewKoma(1, 3, KomaState.OnA, 2);
			onboards[2, 3] = NewKoma(2, 3, KomaState.OnA, 3);
			motoas[0] = onboards[1, 2];
			motoas[1] = onboards[0, 3];
			motoas[2] = onboards[1, 3];
			motoas[3] = onboards[2, 3];

			motoas.CopyTo(komas, 0);
			motobs.CopyTo(komas, 4);

			Draw();
		}
		public virtual void Next()
		{
			switch (gamestate)
			{
				case GameState.turnA:
					gamestate = GameState.turnB;
					break;
				case GameState.turnB:
					gamestate = GameState.turnA;
					break;
				case GameState.over:
					Draw();
					CheckMate();
					return;
			}
			Examine();
			Draw();
		}
		public void Examine()
		{
			Examine(motoas);
			Examine(motobs);
		}
		public void Examine(Koma[] ks)
		{
			bool update = true; int cnt = 0;

			while (update)
			{
				update = false;
				cnt++;
				if (cnt > 1000)
				{
					throw new Exception("Infinite Examine");
				}

				bool[,] poss = new bool[4, 4];
				for (int i = 0; i < 4; i++) for (int g = 0; g < 4; g++) poss[i, g] = false;

				for (int a = 0; a < 4; a++)
				{
					if (!ks[0].GetCanBe(a)) continue;
					for (int b = 0; b < 4; b++)
					{
						if (!ks[1].GetCanBe(b) || b == a) continue;
						for (int c = 0; c < 4; c++)
						{
							if (!ks[2].GetCanBe(c) || c == a || c == b) continue;
							for (int d = 0; d < 4; d++)
							{
								if (!ks[3].GetCanBe(d) || d == a || d == b || d == c) continue;
								poss[0, a] = true; poss[1, b] = true; poss[2, c] = true; poss[3, d] = true;
							}
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					for (int g = 0; g < 4; g++)
					{
						if (!poss[i, g] && ks[i].GetCanBe(g))
						{
							ks[i].SetCanBe(g, false);
							update = true;
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					int n = 0, g0 = 0;
					for (int g = 0; g < 4; g++) if (ks[i].GetCanBe(g)) { n++; g0 = g; }
					if (n == 0)
					{
						throw new Exception("No Gara Possibility");
					}
					if (n == 1 && !ks[i].only)
					{
						ks[i].only = true;
						for (int j = 0; j < 4; j++) if (i != j) ks[j].SetCanBe(g0, false);
						update = true;
					}
				}
			}
		}
		public bool Preexamine()
		{
			return Preexamine(motoas) && Preexamine(motobs);
		}
		public bool Preexamine(Koma[] ks)
		{
			bool update = true; int cnt = 0;

			while (update)
			{
				update = false;
				cnt++;
				if (cnt > 1000)
				{
					throw new Exception("Infinite Examine");
				}

				bool[,] poss = new bool[4, 4];
				for (int i = 0; i < 4; i++) for (int g = 0; g < 4; g++) poss[i, g] = false;

				for (int a = 0; a < 4; a++)
				{
					if (!ks[0].GetCanBe(a)) continue;
					for (int b = 0; b < 4; b++)
					{
						if (!ks[1].GetCanBe(b) || b == a) continue;
						for (int c = 0; c < 4; c++)
						{
							if (!ks[2].GetCanBe(c) || c == a || c == b) continue;
							for (int d = 0; d < 4; d++)
							{
								if (!ks[3].GetCanBe(d) || d == a || d == b || d == c) continue;
								poss[0, a] = true; poss[1, b] = true; poss[2, c] = true; poss[3, d] = true;
							}
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					for (int g = 0; g < 4; g++)
					{
						if (!poss[i, g] && ks[i].GetCanBe(g))
						{
							ks[i].SetCanBe(g, false);
							update = true;
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					int n = 0, g0 = 0;
					for (int g = 0; g < 4; g++) if (ks[i].GetCanBe(g)) { n++; g0 = g; }
					if (n == 0)
					{
						return false;
					}
					if (n == 1 && !ks[i].only)
					{
						ks[i].only = true;
						for (int j = 0; j < 4; j++) if (i != j) ks[j].SetCanBe(g0, false);
						update = true;
					}
				}
			}
			return true;
		}
		public virtual void Tsumi() { }
		public virtual void CheckMate() { }
		public virtual void Draw() { }
		public virtual void Pass() { throw new NotImplementedException(); }
		public List<Te> PossTe()
		{
			List<Te> posste = new List<Te>();
			Te t = new Te();
			foreach (TeState s in Enum.GetValues(typeof(TeState)))
				for (int k = 0; k < 8; k++)
					for (int x = 0; x < 3; x++)
						for (int y = 0; y < 4; y++)
						{
							t.state = s; t.k = k; t.X = x; t.Y = y;
							if (t.IsValid(this)) { posste.Add(t); }
						}
			return posste;
		}
		public void AI()
		{
			if (gamestate == GameState.over) return;
			List<Te> posste = PossTe();
			if (posste.Count == 0)
			{
				throw new Exception("I can do nothing");
			}
			foreach (var te in posste)
			{
				Game game = Clone();
				te.Go(game);
				if (game.gamestate == GameState.over) { te.Go(this); return; }
			}
			Te nextte = new Te(); int max = -big;
			foreach (var te in posste)
			{
				int p = Check(te, Clone(), 0);
				if (max < p)
				{
					max = p;
					nextte = te;
				}
			}
			nextte.Go(this);
		}
		public static int Point(Koma k)
		{
			int res = 0;
			Random r = new Random();
			if (k.canbeH && !k.isNari) { res += r.Next(3, 7); }
			if (k.canbeH && k.isNari) { res += r.Next(12, 18); }
			if (k.canbeK) { res += r.Next(6, 14); }
			if (k.canbeZ) { res += r.Next(6, 14); }
			if (k.canbeL) { res += r.Next(9, 11); }
			return res;
		}
		public static int Check(Te te, Game game, int depth)
		{
			te.Go(game);
			if (game.gamestate == GameState.over)
			{
				return big;
			}
			if (depth == 1)
			{
				List<Koma> mochi = game.gamestate == GameState.turnB ? game.mochias : game.mochibs;
				int res = 0;
				foreach (var k in mochi) res += Point(k);
				foreach (var k in game.onboards)
				{
					if (k == null) continue;
					if (k.state == ((game.gamestate == GameState.turnB) ? KomaState.OnA : KomaState.OnB))
					{
						res += Point(k);
					}
				}
				return res;
			}
			int ans = big;
			if (game.IsTsumi())
			{
				return big;
			}
			foreach (var te2 in game.PossTe())
			{
				int res = big;
				Game game2 = game.Clone();
				te2.Go(game2);
				if (game2.gamestate == GameState.over) { ans = 0; continue; }
				foreach (var te3 in game2.PossTe())
				{
					int p = Check(te3, game2.Clone(), depth + 1);
					res = Math.Min(res, p);
				}
				ans = Math.Min(ans, res);
			}
			return ans;
		}
		public bool IsTsumi()
		{
			if (gamestate == GameState.over) return false;
			List<Te> posste = PossTe();
			int n = posste.Count;
			if (n == 0)
			{
				return false;
			}
			foreach (var te in posste)
			{
				Game game = Clone();
				te.Go(game);
				if (game.gamestate == GameState.over) { return false; }
				else
				{
					bool lose = false;
					List<Te> aitete = game.PossTe();
					foreach (var te2 in aitete)
					{
						Game game2 = game.Clone();
						te2.Go(game2);
						if (game2.gamestate == GameState.over)
						{
							lose = true;
						}
					}
					if (!lose) return false;
				}
			}
			return true;
		}
		public Game Clone()
		{
			Game game = new Game();
			game.gamestate = gamestate;
			Dictionary<Koma, Koma> dict = new Dictionary<Koma, Koma>();
			for (int i = 0; i < 8; i++) dict.Add(komas[i], komas[i].Clone(game));
			for (int x = 0; x < 3; x++) for (int y = 0; y < 4; y++) if (onboards[x, y] != null) game.onboards[x, y] = dict[onboards[x, y]];
			for (int i = 0; i < 4; i++) game.motoas[i] = dict[motoas[i]];
			for (int i = 0; i < 4; i++) game.motobs[i] = dict[motobs[i]];
			for (int i = 0; i < 8; i++) game.komas[i] = dict[komas[i]];
			game.mochias = mochias.ConvertAll(k => dict[k]);
			game.mochibs = mochibs.ConvertAll(k => dict[k]);
			return game;
		}
	}

	public class GGame : Game
	{
		public const double wakuwidth = 100, wakuheight = 100, komawidth = 80, komaheight = 80, mochiwidth = 700, mochiheight = 100;
		public MainWindow window;
		public Canvas mochiA, mochiB, board, large;
		public TextBlock mojiA, mojiB;
		public GKoma active;

		public GGame(MainWindow window)
		{
			this.window = window;
			mochiA = window.mochiA; mochiB = window.mochiB; board = window.board; large = window.large;
			mojiA = window.mojiA; mojiB = window.mojiB;
			window.MouseMove += MouseMove;
		}
		public override Koma NewKoma(int X, int Y, KomaState state, int index)
		{
			return new GKoma(X, Y, state, index, this);
		}
		public override void CheckMate()
		{
			switch (winner)
			{
				case GameState.turnA:
					MessageBox.Show("後手のライオンが取られました。先手の勝ちです。");
					break;
				case GameState.turnB:
					MessageBox.Show("先手のライオンが取られました。後手の勝ちです。");
					break;
				default:
					throw new Exception("error");
			}
		}
		public override void Tsumi()
		{
			switch (gamestate)
			{
				case GameState.turnA:
					MessageBox.Show("先手はどうしてもライオンを取られます。詰みです。");
					gamestate = GameState.over;
					winner = GameState.turnB;
					break;
				case GameState.turnB:
					MessageBox.Show("後手はどうしてもライオンを取られます。詰みです。");
					gamestate = GameState.over;
					winner = GameState.turnA;
					break;
				default:
					throw new Exception("error");
			}
			Draw();
		}
		public override void Draw()
		{
			mochiA.Children.Clear(); mochiB.Children.Clear();
			board.Children.Clear();

			int na = mochias.Count;
			for (int i = 0; i < na; i++)
			{
				var square = mochias[i].Draw();
				mochiA.Children.Add(square);
				Canvas.SetLeft(square, (mochiwidth - na * wakuwidth) / 2 + (wakuwidth - komawidth) / 2 + wakuwidth * i);
				Canvas.SetTop(square, (mochiheight - komaheight) / 2);
			}

			int nb = mochibs.Count;
			for (int i = 0; i < nb; i++)
			{
				var square = mochibs[i].Draw();
				mochiB.Children.Add(square);
				Canvas.SetLeft(square, (mochiwidth - nb * wakuwidth) / 2 + (wakuwidth - komawidth) / 2 + wakuwidth * i);
				Canvas.SetTop(square, (mochiheight - komaheight) / 2);
			}

			for (int x = 0; x < 3; x++)
				for (int y = 0; y < 4; y++)
				{
					//枠
					var waku = new Rectangle();
					waku.Width = wakuwidth; waku.Height = wakuheight;
					waku.Stroke = Brushes.Black;
					waku.StrokeThickness = 1;
					var dbl = new DoubleCollection(); dbl.Add(2); dbl.Add(2);
					waku.StrokeDashArray = dbl;
					board.Children.Add(waku);
					Canvas.SetLeft(waku, x * wakuwidth);
					Canvas.SetTop(waku, y * wakuheight);
					Canvas.SetZIndex(waku, 0);

					//駒
					if (onboards[x, y] == null) continue;
					var square = onboards[x, y].Draw();
					board.Children.Add(square);
					Canvas.SetLeft(square, x * wakuwidth + (wakuwidth - komawidth) / 2);
					Canvas.SetTop(square, y * wakuheight + (wakuheight - komaheight) / 2);
					Canvas.SetZIndex(square, 1);
				}

			switch (gamestate)
			{
				case GameState.turnA:
					mojiA.Foreground = Brushes.DeepPink;
					mojiB.Foreground = Brushes.Black;
					break;
				case GameState.turnB:
					mojiB.Foreground = Brushes.DeepPink;
					mojiA.Foreground = Brushes.Black;
					break;
				case GameState.over:
					switch (winner)
					{
						case GameState.turnA:
							mojiA.Foreground = Brushes.Red;
							mojiB.Foreground = Brushes.Gray;
							break;
						case GameState.turnB:
							mojiB.Foreground = Brushes.Red;
							mojiA.Foreground = Brushes.Gray;
							break;
						default:
							throw new Exception("error");
					}
					break;
			}
		}
		public override void Pass()
		{
			switch (gamestate)
			{
				case GameState.turnA:
					MessageBox.Show("先手はパスします。");
					break;
				case GameState.turnB:
					MessageBox.Show("後手はパスします。");
					break;
				default:
					throw new Exception("error");
			}
			Next();
		}
		public override void Next()
		{
			base.Next();
			if (IsTsumi()) Tsumi();
		}
		private void MouseMove(object sender, MouseEventArgs e)
		{
			if (active != null)
			{
				var p = e.GetPosition(large);
				Canvas.SetLeft(active.square, p.X - komawidth / 2);
				Canvas.SetTop(active.square, p.Y - komaheight / 2);
			}
		}
	}

	public partial class MainWindow : Window
	{
		public GGame ggame;

		public MainWindow()
		{
			InitializeComponent();
			ggame = new GGame(this);
			NewGame();
		}
		public void NewGame()
		{
			ggame.NewGame();
		}
		public void NewGame(object sender, ExecutedRoutedEventArgs e)
		{
			NewGame();
		}
		public void AI()
		{
			ggame.AI();
		}
		public void AI(object sender, ExecutedRoutedEventArgs e)
		{
			AI();
		}
		public void Pass()
		{
			ggame.Pass();
		}
		private void Pass(object sender, ExecutedRoutedEventArgs e)
		{
			Pass();
		}
	}
}
