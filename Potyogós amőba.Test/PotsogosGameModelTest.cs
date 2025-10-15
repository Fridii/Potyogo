using Potyogós_amőba.Model;
using Potyogós_amőba.Persistence;
using Moq;
namespace Potyogós_amőba.Test
{
    [TestClass]
    public class PotsogosGameModelTest
    {
        private PotyogosGameModel _model = null!; // a tesztelendő modell
        private PotyogosTable _mockedTable = null!; // mockolt játéktábla
        private Mock<IPotyogosDataAccess> _mock = null!; // az adatelérés mock-ja
        private MockTimer _mockedTimer = new MockTimer(); // az időzítő mock-ja

        [TestInitialize]
        public void Initialize()
        {
            _mockedTable = new PotyogosTable(9,10,31);
            _mockedTable.AddElement( 2, Field.PlayerX);
            _mockedTable.AddElement(1,  Field.PlayerO);
            _mockedTable.AddElement(2,  Field.PlayerX);

            _mock = new Mock<IPotyogosDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult(_mockedTable));
            // a mock a LoadAsync műveletben bármilyen paraméterre az előre beállított játéktáblát fogja visszaadni

            _model = new PotyogosGameModel(_mock.Object, _mockedTimer);
            // példányosítjuk a modellt a mock objektummal

            _model.GameRefresh += new EventHandler<PotyogosEventArgs>(Model_GameRefresh);
            _model.GameOver += new EventHandler<PotyogosEventArgs>(Model_GameOver);
        }


        

      
        [TestMethod]
        public void PotyogosGameModelNewGame()
        {
            Int32 size = 5;
            _model.NewGame(size);

            Assert.AreEqual(size, _model.TableSize); 
            Assert.AreEqual(_model.CurrentPlayer, Field.PlayerX); 
            Assert.AreEqual(0, _model.PlayerTimeX); 
            Assert.AreEqual(0, _model.PlayerTimeO); 

            Int32 emptyFields = 0;
            for (Int32 i = 0; i < size; i++)
                for (Int32 j = 0; j < size; j++)
                    if (_model.IsEmpty(i, j))
                        emptyFields++;

            Assert.AreEqual(25, emptyFields); // szabad mezők száma is megfelelő
        }

        [TestMethod]
        public void PotyogosGameModelDropToken()
        {
            Assert.AreEqual(Field.PlayerX ,  _model.CurrentPlayer); 

            _model.DropToken(2);

            Assert.AreEqual(Field.PlayerX, _model.CurrentPlayer); // mivel a játék áll, nem szabad, hogy lépjünk
            

            _model.NewGame(9);

            Random random = new Random();
            Int32 y = 0;int x = 8;
            do
            {
                y = random.Next(0, 9);
            } while (!_model.IsEmpty(x,y));

            _model.DropToken(y);

            Assert.AreEqual(Field.PlayerO, _model.CurrentPlayer); // most már léptünk
            Assert.AreNotEqual(Field.Ures, _model[x, y]); // kitöltöttnek kell lennie

            Assert.AreEqual(0, _model.PlayerTimeX); // az idő viszont nem változott
            Assert.AreEqual(0, _model.PlayerTimeO);

            
            for ( ; x>0; x--) // további 8 lépés végrehajtása
            {
                _model.DropToken(y);
                Assert.IsTrue(_model[x, y] == _model.CurrentPlayer); // mindig az aktuális játékos léphet a mezőre 
            }

            _model.DropToken(y);
            Assert.AreEqual(_model[x+1,y], Field.PlayerO);// továbbra is az O kell jojjon mert a 10. lépés helytelen volt (teli oszlop)
        }
        [TestMethod]
        public void PotyogosStepNumberTest()
        {
            _model.NewGame(6);
            Assert.AreEqual(0, _model.Steps);
            Int32 k = 0;
            for (Int32 i = 0; i < 3; i++)
                for (Int32 j = 0; j < 6; j++)
                {
                    _model.DropToken(j);
                    k++;
                    Assert.AreEqual(k, _model.Steps); // lépésszám megfelelő-e
                }
        }
        [TestMethod]
        public void PotyogosGameWonTest()
        {
            _model.NewGame(6);
           
            bool eventRaised = false;
            _model.GameOver += delegate (object? sender, PotyogosEventArgs e)
            {
                eventRaised = true;
                Assert.IsTrue(e.CurrantPlayer == Field.PlayerX); // a megfelelő játékos győzött-e
            };

            for (Int32 i = 0; i < 3; i++)
                for (Int32 j = 0; j < 6; j++)
                {
                    _model.DropToken(j);
                }
            _model.DropToken(0); // ezzel a lépéssel nyer X

            Assert.IsTrue(eventRaised); // kiváltottuk-e az eseményt
        }

        [TestMethod]
        public void PotyogosGameModelGameRefresh()
        {
            _model.NewGame(5);

            Int32 time = _model.PlayerTimeX;
            while (time<10)
            {
                _mockedTimer.RaiseElapsed(); //kézzel kiváltja az időzítő eseményét, így nem kell valódi időt várni

                time++;

                Assert.AreEqual(time, _model.PlayerTimeX); //az idő nőtt
                Assert.AreEqual(Field.PlayerX, _model.CurrentPlayer); // de a játékos nem változott
                Assert.AreEqual(0, _model.PlayerTimeO); // másik játékos ideje nem változott
            }
            

        }

        [TestMethod]
        public async Task PotyogosGameModelLoadTest()
        {
            
            _model.NewGame(9);

            // betöltünk egy játékot
            await _model.LoadGameAsync(String.Empty);

            for (Int32 i = 0; i < 3; i++)
                for (Int32 j = 0; j < 3; j++)
                {
                    Assert.AreEqual(_mockedTable.GetValue(i, j), _model.GetValue(i, j));
                    // ellenőrizzük, valamennyi mező értéke megfelelő-e
                   
                }

            // az idő is bállítodik
            Assert.AreEqual(_mockedTable.PlayerStartTimeX, _model.PlayerTimeX);
            Assert.AreEqual(_mockedTable.PlayerStartTimeO, _model.PlayerTimeO);

            // ellenőrizzük, hogy meghívták-e a Load műveletet a megadott paraméterrel
            _mock.Verify(dataAccess => dataAccess.LoadAsync(String.Empty), Times.Once());
        }

        private void Model_GameRefresh(Object? sender, PotyogosEventArgs e)
        {
            Assert.AreEqual(e.CurrantPlayer, _model.CurrentPlayer); 
            Assert.AreEqual(e.GameTime, _model.CurrentPlayer==Field.PlayerX ? _model.PlayerTimeX : _model.PlayerTimeO); // a két értéknek egyeznie kell
       
        }

        private void Model_GameOver(Object? sender, PotyogosEventArgs e)
        {
            Assert.IsTrue(_model.isGameOver); // biztosan vége van a játéknak
            
        }
    }
}
