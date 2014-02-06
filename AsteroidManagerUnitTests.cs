using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Xasteroids
{
    [TestFixture]
    public class AsteroidManagerUnitTests
    {

        [Test]
        public void AsteroidsListConfig_Test()
        {
            Random r = new Random();
            BlackAsteroid blacky = new BlackAsteroid(600, 1000, r);
            GraviticAsteroid gravy = new GraviticAsteroid(1000, 600, r);
            AsteroidsList theList = new AsteroidsList { Asteroids = new List<Asteroid> { blacky, gravy } };
            AsteroidsList fromConfig = new AsteroidsList { Configuration = theList.Configuration };
            Assert.That(fromConfig.Asteroids[0].GetType().Equals(blacky.GetType()));
            Assert.That(fromConfig.Asteroids[1].GetType().Equals(gravy.GetType()));
        }
    }
}
