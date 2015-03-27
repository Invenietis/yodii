using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Model;
using Yodii.ObjectExplorer.ViewModels;
using Yodii.Wpf.Tests;

namespace Yodii.ObjectExplorer.Tests
{
    [TestFixture]
    public class PluginViewModelTests
    {
        [Test]
        public void PluginViewModel_CanBeInstanciated()
        {
            PluginViewModel vm = new PluginViewModel();

            Assert.That( vm.Plugin, Is.Null );
        }

        [Test]
        public void PluginViewModel_CanLoadPlugin()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                CollectionAssert.IsNotEmpty( ctx.Engine.LiveInfo.Plugins );
                ILivePluginInfo s = ctx.Engine.LiveInfo.Plugins.First();

                PluginViewModel vm = new PluginViewModel();
                vm.LoadLiveItem( s );
                Assert.That( vm.Plugin, Is.Not.Null );
            }
        }

        [Test]
        public void PluginViewModel_CannotLoadPluginTwice()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                CollectionAssert.IsNotEmpty( ctx.Engine.LiveInfo.Plugins );
                ILivePluginInfo s = ctx.Engine.LiveInfo.Plugins.First();

                PluginViewModel vm = new PluginViewModel();
                vm.LoadLiveItem( s );
                Assert.Throws<InvalidOperationException>( () => vm.LoadLiveItem( s ) );
            }
        }

        [Test]
        public void PluginViewModel_CorrectlyLoads_DisplayAttributeData()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                ILivePluginInfo s = ctx.Engine.LiveInfo.FindPlugin( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.PluginWithDisplayAttribute" );
                Assert.That( s, Is.Not.Null );

                PluginViewModel vm = new PluginViewModel();
                vm.LoadLiveItem( s );

                Assert.That( vm.DisplayName, Is.EqualTo( "Yodii item (with display attribute)" ), "DisplayName should be retrieved from Display attribute's Name property" );
                Assert.That( vm.Description, Is.EqualTo( "Some test item with a name and description." ), "Description should be retrieved from Display attribute" );
                Assert.That( vm.FullName, Is.EqualTo( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.PluginWithDisplayAttribute" ), "FullName is equal to the item's FullName" );
            }
        }
        [Test]
        public void PluginViewModel_CorrectlyLoadsDefaultProperties_WithoutDisplayAttributeData()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                ILivePluginInfo s = ctx.Engine.LiveInfo.FindPlugin( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.MyYodiiPlugin" );
                Assert.That( s, Is.Not.Null );

                PluginViewModel vm = new PluginViewModel();
                vm.LoadLiveItem( s );

                Assert.That( vm.DisplayName, Is.EqualTo( "MyYodiiPlugin" ), "DisplayName should be the class name without namespace when Display attribute's Name property is not used" );
                Assert.That( vm.Description, Is.EqualTo( String.Empty ), "Description should be empty when Display's Description is unused" );
                Assert.That( vm.FullName, Is.EqualTo( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.MyYodiiPlugin" ), "FullName is equal to the item's FullName" );
            }
        }
    }
}
