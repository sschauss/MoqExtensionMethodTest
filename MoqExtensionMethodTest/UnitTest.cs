using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;

namespace MoqExtensionMethodTest
{
    public interface ISomeInterface
    {
        T DoInterfaceStuff<T>(object parameter);
        T2 DoInterfaceStuff<T1, T2>(T1 parameter1, T2 parameter2);
    }
    
    public static class ExtensionClass
    {
        public static TResult DoExtensionStuff<TResult>(this ISomeInterface someInterface, object initialObject,
            params object[] objects)
        {
            var result = someInterface.DoInterfaceStuff<TResult>(initialObject);
            return objects.Aggregate(result, (agg, cur) => someInterface.DoInterfaceStuff(cur, agg));
        }
    }

    public class SourceClassA
    {
        public int Integer { get; set; }
    }

    public class SourceClassB
    {
        public string String { get; set; }
    }

    public class TargetClass
    {
        public int Integer { get; set; }
        public string String { get; set; }
    }

    public class SomeClass
    {
        private readonly ISomeInterface _someInterface;

        public SomeClass(ISomeInterface someInterface)
        {
            _someInterface = someInterface;
        }

        public TargetClass DoClassStuff(SourceClassA sourceClassA, SourceClassB sourceClassB)
        {
            return _someInterface.DoExtensionStuff<TargetClass>(sourceClassA, sourceClassB);
        }
    }
    
    public class UnitTest
    {
        private readonly SomeClass _sut;
        private readonly SourceClassA _sourceA;
        private readonly SourceClassB _sourceB;
        private readonly TargetClass _target;
        private readonly Mock<ISomeInterface> _someInterfaceMock;

        public UnitTest()
        {
            _sourceA = new SourceClassA
            {
                Integer = 1
            };
            _sourceB = new SourceClassB
            {
                String = "stringB"
            };
            _target = new TargetClass
            {
                Integer = 2,
                String = "stringT"
            };
            _someInterfaceMock = new Mock<ISomeInterface>();
            _someInterfaceMock.Setup(m => m.DoInterfaceStuff<TargetClass>(_sourceA)).Returns(_target);
            _someInterfaceMock.Setup(m => m.DoInterfaceStuff(_sourceB, _target)).Returns(_target);
            _sut = new SomeClass(_someInterfaceMock.Object);
        }
        
        [Fact]
        public void TestDoClassStuff()
        {
            var result = _sut.DoClassStuff(_sourceA, _sourceB);
            
            result.Should().BeEquivalentTo(_target);
        }
        
        [Fact]
        public void TestMockSetupSourceClassA()
        {
            var result = _someInterfaceMock.Object.DoInterfaceStuff<TargetClass>(_sourceA);
            
            result.Should().BeEquivalentTo(_target);
        }
        
        [Fact]
        public void TestMockSetupSourceClassB()
        {
            var result = _someInterfaceMock.Object.DoInterfaceStuff(_sourceB, _target);
            
            result.Should().BeEquivalentTo(_target);
        }
    }
}