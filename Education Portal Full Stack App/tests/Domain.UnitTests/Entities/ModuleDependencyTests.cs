using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>A dependency links two saved modules, never a module to itself; loops across several modules are found by the graph.</summary>
public class ModuleDependencyTests
{
    [Test]
    public void ShouldLinkTwoModules()
    {
        var dependency = ModuleDependency.Create(Module("FEES", 2), Module("STUDENTS", 1));

        dependency.ModuleId.ShouldBe(2);
        dependency.RequiresModuleId.ShouldBe(1);
    }

    [Test]
    public void ShouldRefuseAModuleRequiringItself()
    {
        var fees = Module("FEES", 2);

        Should.Throw<ArgumentException>(() => ModuleDependency.Create(fees, fees));
    }

    [Test]
    public void ShouldRefuseAnUnsavedModule()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => ModuleDependency.Create(Module("FEES", 0), Module("STUDENTS", 1)));
    }

    // Two edges: A -> B (A requires B) and B -> C.
    private static readonly (int, int)[] Chain = [(1, 2), (2, 3)];

    [TestCase(3, 1, true)] // C -> A closes A -> B -> C -> A
    [TestCase(2, 1, true)] // B -> A closes A -> B -> A
    [TestCase(1, 1, true)] // a module requiring itself
    [TestCase(1, 3, false)] // A -> C only shortcuts the chain
    [TestCase(4, 1, false)] // a new module requiring A
    [TestCase(1, 4, false)] // A requiring a new module
    public void ShouldFindLoops(int moduleId, int requiresId, bool loops)
    {
        ModuleDependencyGraph.WouldCreateCycle(Chain, moduleId, requiresId).ShouldBe(loops);
    }

    [Test]
    public void ShouldFindNoLoopWithoutDependencies()
    {
        ModuleDependencyGraph.WouldCreateCycle([], 1, 2).ShouldBeFalse();
    }

    [Test]
    public void ShouldCollectEverythingAModuleRequiresEvenOnALoopedGraph()
    {
        (int, int)[] looped = [(1, 2), (2, 3), (3, 1), (4, 5)];

        ModuleDependencyGraph.RequiredBy(Chain, [1]).ShouldBe([1, 2, 3], ignoreOrder: true);
        ModuleDependencyGraph.RequiredBy(looped, [2]).ShouldBe([1, 2, 3], ignoreOrder: true);
        ModuleDependencyGraph.RequiredBy(looped, [4, 3]).ShouldBe([1, 2, 3, 4, 5], ignoreOrder: true);
        ModuleDependencyGraph.RequiredBy(looped, []).ShouldBeEmpty();
    }

    private static PortalModule Module(string code, int id)
    {
        var module = PortalModule.Create(code, code, "", isCore: false, sortOrder: 0);
        module.Id = id;
        return module;
    }
}
