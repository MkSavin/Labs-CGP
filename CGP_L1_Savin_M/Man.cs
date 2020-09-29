using System;
using System.Collections.Generic;
using System.Text;

namespace CGP_L1_Savin_M {
    class Man {

        private string name;
        private uint age;
        private float health;
        private float money;
        private bool isAlive;

        private static Random rand = new Random();

        public Man(string _name) {
            name = _name;
            isAlive = true;

            age = (uint)rand.Next(15, 50);
            health = rand.Next(10, 100);

            money = rand.Next(1, 100);
        }

        public string GetPhrase(int id) => id switch {
            1 => "Привет, меня зовут " + name + ", рад познакомиться",
            2 => "Мне " + age + ". А тебе?",
            3 => health > 40 ? "Да я зводоров как бык!" : "Со здоровьем у меня неважно, дожить бы до " + (age + 10).ToString(),
            _ => "",
        };

        public void Talk() => Console.WriteLine(
            GetPhrase(rand.Next(1, 4))
        );

        public void Go() => Console.WriteLine(
            isAlive ? (health > 40 ? name + " мирно прогуливается по городу" : name + " болен и не может гулять по городу") : name + " не может идти, он умер"
        );

        public void Kill() => isAlive = false;

        public bool IsAlive() => isAlive;

        public float Health() => health;

        public float Money() => money;

        public void Work() {
            money += rand.Next(0, 5);
            health -= rand.Next(0, 2);
        }

        public void BuyAspirine() {
            money -= 10;
            health += 10;
        }

    }
}
