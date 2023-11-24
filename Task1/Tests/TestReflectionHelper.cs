using ReflectionLib;

namespace Tests
{
    public class A
    {
        public B B { get; set; }
    }

    public class B
    {
        public C[] C { get; set; }
        public override string ToString()
        {
            return $"B(C={C})";
        }
    }

    public class C
    {
        public string Name { get; set; }

        public D[] D { get; set; }
        public override string ToString()
        {
            string[] dStrings = new string[D.Length];
            for (int i = 0; i < D.Length; i++)
            {
                dStrings[i] = D[i].ToString();
            }

            return $"C(Name={Name}, D={string.Join(", ", dStrings)})";
        }
    }

    public class D
    {
        public int Years { get; set; }

        public string[] Langs { get; set; }
        public override string ToString()
        {
            return $"D(Years={Years}, Langs={string.Join(", ", Langs)})";
        }
    }

    public class MyClass
    {
        public int Number { get; set; }
        public MyNestedClass Nested { get; set; }
        public List<MyNestedClass> NestedList { get; set; }
    }

    public class MyNestedClass
    {
        public string Text { get; set; }
        public D[] D { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public string Gender;
        public Address Address { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }

    public class TestReflectionHelper
    {
            
        [Fact]
        public void TestMakeGetter1()
        {
            //Пункт A

            Address address = new Address
            {
                Street = "Main Street",
                City = "New York"
            };

            Person person = new Person
            {
                Name = "John",
                Age = 30,
                Address = address,
                Gender = "Male"
            };

            var getAge = ReflectionHelper.MakeGetter<Person, int>("Age");
            var age = getAge(person);
            Assert.Equal(30, age);

            var getGender = ReflectionHelper.MakeGetter<Person, string>("Gender");
            var gender = getGender(person);
            Assert.Equal("Male", gender);

            var getCity = ReflectionHelper.MakeGetter<Person, string>("Address.City");
            var city = getCity(person);
            Assert.Equal("New York", city);

        }

        [Fact]
        public void TestMakeGetter2()
        {
            var obj = new A
            {
                B = new B
                {
                    C = [
                        new C
                        {
                            Name = "Nikita",
                            D = [
                                new D { Langs = ["C#", "Python"], Years = 21 },
                                new D { Langs = ["OCaml", "C++"], Years = 21 },
                            ]
                        },
                        new C
                        {
                            Name = "Alex",
                            D = [
                                new D { Langs = ["Asm", "Python"], Years = 30 },
                                new D { Langs = ["C", "Matlab"], Years = 30 },
                            ]
                        }
                    ]
                }
            };

            var arr_elem_getter_1 = ReflectionHelper.MakeGetter<A, string>("B.C[0].D[1].Langs[0]");
            var result_1 = arr_elem_getter_1(obj);
            Assert.Equal("OCaml", result_1);

            var arr_elem_getter_2 = ReflectionHelper.MakeGetter<A, string>("B.C[1].D[1].Langs[1]");
            var result_2 = arr_elem_getter_2(obj);
            Assert.Equal("Matlab", result_2);

            var c1Getter = ReflectionHelper.MakeGetter<A, C>("B.C[1]");
            var c1 = c1Getter(obj);
            Assert.Equal("C(Name=Alex, D=D(Years=30, Langs=Asm, Python), D(Years=30, Langs=C, Matlab))", c1.ToString());

            // Ошибка при выходе за границы массива
            var badGetter = ReflectionHelper.MakeGetter<A, string>("B.C[1].D[3].Langs[1]");
            Assert.Throws<IndexOutOfRangeException>(() => badGetter(obj));

        }

        [Fact]
        public void TestMakeGetter3()
        { 
            var obj1 = new MyClass
            {
                Number = 42,
                Nested = new MyNestedClass { Text = "Hello World" },
                NestedList = new List<MyNestedClass>
            {
                new MyNestedClass { 
                    Text = "Foo",
                    D = [
                        new D { Langs = ["C#", "Python"], Years = 21 },
                        new D { Langs = ["OCaml", "C++"], Years = 21 },
                    ]
                },
                new MyNestedClass { 
                    Text = "Bar",
                    D = [
                        new D { Langs = ["Asm", "Python"], Years = 30 },
                        new D { Langs = ["C", "Matlab"], Years = 30 },
                    ]
                }
            }
            };

            var textGetter = ReflectionHelper.MakeGetter<MyClass, string>("Nested.Text");
            var nestedText = textGetter(obj1);
            Assert.Equal("Hello World", nestedText);

            textGetter = ReflectionHelper.MakeGetter<MyClass, string>("NestedList[0].Text");
            var listText = textGetter(obj1);
            Assert.Equal("Foo", listText);

            var yearsGetter = ReflectionHelper.MakeGetter<MyClass, int>("NestedList[0].D[1].Years");
            var years = yearsGetter(obj1);
            Assert.Equal(21, years);

            var langGetter = ReflectionHelper.MakeGetter<MyClass, string>("NestedList[0].D[0].Langs[0]");
            var lang = langGetter(obj1);
            Assert.Equal("C#", lang);
        }
    }
}
