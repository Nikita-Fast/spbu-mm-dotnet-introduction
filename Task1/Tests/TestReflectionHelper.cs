using NReflectionHelper;

namespace Tests
{
    public class A
    {
        public B B { get; set; }
    }

    public class B
    {
        public C C { get; set; }
        public override string ToString()
        {
            return $"B(C={C})";
        }
    }

    public class C
    {
        public string Name { get; set; }

        public D D { get; set; }
        public override string ToString()
        {
            return $"C(Name={Name}, D={D})";
        }
    }

    public class D
    {
        public int Years { get; set; }
        public override string ToString()
        {
            return $"D(Years={Years})";
        }
    }
    public class TestReflectionHelper
    {
        [Fact]
        public void TestMakeGetter()
        {
            var obj = new A
            {
                B = new B
                {
                    C = new C
                    {
                        Name = "Nikita",
                        D = new D
                        {
                            Years = 21
                        }
                    }
                }
            };

            // успешный доступ к полю Years класса D
            var yearsGetter = ReflectionHelper.MakeGetter<A, int>("B.C.D.Years");
            var years = yearsGetter(obj);
            Assert.Equal(21, years);

            // успешный доступ к полю Name класса C
            var nameGetter = ReflectionHelper.MakeGetter<A, string>("B.C.Name");
            var name = nameGetter(obj);
            Assert.Equal("Nikita", name);

            // успешный доступ к полю D класса C
            var dGetter = ReflectionHelper.MakeGetter<A, D>("B.C.D");
            var d = dGetter(obj);
            Assert.Equal("D(Years=21)", d.ToString());

            // успешный доступ к полю B класса A
            var bGetter = ReflectionHelper.MakeGetter<A, B>("B");
            var b = bGetter(obj);
            Assert.Equal("B(C=C(Name=Nikita, D=D(Years=21)))", b.ToString());

            // ошибка при попытке обращение к несуществующему полю X
            Assert.Throws<ArgumentException>(() => ReflectionHelper.MakeGetter<A, int>("B.C.D.X"));

            // указали неверный тип у поля Years и получили ошибку
            Assert.Throws<ArgumentException>(() => ReflectionHelper.MakeGetter<A, string>("B.C.D.Years"));
        }
    }
}