using ITV.Models;

namespace ITV.Factories;

public static class FactoryCitas
{
    public static List<Cita> Seed()
    {
        return new List<Cita>()
        {
            // 1
            new Cita(new DateTime(2018, 5, 20), DateTime.Today.AddDays(1), "1234BDF", "Seat", "Ibiza", 1.2,
                Motor.Gasolina, "12345678Z"),

            // 2
            new Cita(new DateTime(2021, 2, 10), DateTime.Today.AddDays(2), "5678DFG", "BMW", "Serie X", 3.0,
                Motor.Diesel, "87654321X"),

            // 3
            new Cita(new DateTime(2023, 11, 15), DateTime.Today.AddDays(3), "9012GHJ", "Tesla", "Model Tres", 50.0,
                Motor.Electrico, "72430770K"),

            // 4
            new Cita(new DateTime(1985, 06, 30), DateTime.Today.AddDays(4), "1122KKK", "Volkswagen", "Golf", 1.8,
                Motor.Gasolina, "83307183X"),

            // 5
            new Cita(new DateTime(2020, 01, 12), DateTime.Today.AddDays(5), "3456LLL", "Toyota", "Yaris", 1.5,
                Motor.Gasolina, "10157001V"),

            // 6
            new Cita(new DateTime(2019, 08, 05), DateTime.Today.AddDays(6), "7890MMM", "Ford", "Transit", 2.0,
                Motor.Diesel, "18676482E"),

            // 7
            new Cita(new DateTime(2022, 04, 22), DateTime.Today.AddDays(7), "5357PPR", "Porsche", "Carrera", 3.0,
                Motor.Gasolina, "45256386E"),

// 8
            new Cita(new DateTime(2015, 09, 10), DateTime.Today.AddDays(8), "2468STV", "Renault", "Clio", 0.9,
                Motor.Gasolina, "99041917F"),

// 9
            new Cita(new DateTime(2017, 12, 01), DateTime.Today.AddDays(9), "9753VWX", "Jeep", "Wrangler", 2.8,
                Motor.Diesel, "58746765G"),

// 10
            new Cita(new DateTime(2016, 03, 18), DateTime.Today.AddDays(10), "8642YZZ", "Audi", "ABB", 2.0,
                Motor.Diesel, "36436598K"),

// 11
            new Cita(new DateTime(2021, 07, 25), DateTime.Today.AddDays(11), "1111BBB", "Mazda", "ABB", 2.0,
                Motor.Gasolina, "92478954W"),

// 12
            new Cita(new DateTime(2023, 05, 05), DateTime.Today.AddDays(12), "2222CCC", "Hyundai", "Tucson", 1.6,
                Motor.Hidrogeno, "59966704T"),

// 13
            new Cita(new DateTime(2020, 10, 30), DateTime.Today.AddDays(13), "3333DDD", "Smart", "Fortwo", 1.0,
                Motor.Electrico, "04707953Z"),

// 14
            new Cita(new DateTime(2018, 11, 11), DateTime.Today.AddDays(14), "4444FFF", "Toyota", "Hilux", 2.4,
                Motor.Diesel, "10868795F"),

// 15
            new Cita(new DateTime(2019, 02, 14), DateTime.Today.AddDays(15), "5555GGG", "Jaguar", "XEEE", 2.0,
                Motor.Gasolina, "97321048K")
        };
    }
}