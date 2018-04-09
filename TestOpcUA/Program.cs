using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akomi.DynamicOpcUa;
using Akomi.InformationModel.Device;
using Akomi.InformationModel.Component.ComponentPort;
using Tapako.DeviceInformationManagement.InformationSources;
using Akomi.InformationModel.Skills.SkillCatalogue;
using MH5F;
using Rexroth_3842999898;
using Akomi.InformationModel.Component.Connection;
using RetainingCylinder;
using Akomi.InformationModel.Skills;
using Akomi.InformationModel.Component;
using Akomi.InformationModel.Component.PhysicalDescription;

namespace TestOpcUA
{
    public class Program
    {
        static void Main(string[] args)
        {
            #region PortTest
            var port = new ComponentPort();
            var feature = new MechanicalElectricalFeature();
            port.Mechanical = new Mechanical();
            //port.Mechanical.Feature.Add(feature);
            #endregion

            #region Device
            IDevice robot = new DeviceBase();
            IDeviceCompletement robotCompletement = new Mh5FCompletement();
            robot = robotCompletement.CompleteDeviceDriver(ref robot);
            robot.Identification.SerialNumber = "R145455863";

            ((SkillParameter<CartesianCoSy>)robot.Skills.GetSkill<SkillMoveBase>().GetParameter("TargetPose", Direction.Input)).Value = new CartesianCoSy() { Unit = "mm" };

            IDevice retainer = new DeviceBase();
            IDeviceCompletement retainerCompletement = new RetainingCylinder._384252240Completement();
            retainer = retainerCompletement.CompleteDeviceDriver(ref retainer);
            retainer.Identification.SerialNumber = "1";
            retainer.PresentationData.BrowseName = "Retainer";
            
            #endregion

            #region opc
            OpcUaServer server = new OpcUaServer();
            OpcUaStartupParameters opcUaParams = new OpcUaStartupParameters()
            { RootNodeName = robot.PresentationData.BrowseName,
                
            };

            OpcUaServer server2 = new OpcUaServer();
            OpcUaStartupParameters opcUaParams2 = new OpcUaStartupParameters()
            {
                RootNodeName = retainer.PresentationData.BrowseName,
                IgnoreNullPointer=false
            };
            var physDescr = new PhysicalDescription() { Pose = new CartesianCoSy() };
            ((SkillParameter<IComponent>)retainer.Skills.FirstOrDefault().Parameters.First(x => x.Name == "Material" && x.Direction == Direction.Input)).Value = new Akomi.InformationModel.Product.ProductBase() { PhysicalDescription = physDescr };
            ((SkillParameter<IComponent>)robot.Skills.FirstOrDefault().Parameters.First(x => x.Name == "Material" && x.Direction == Direction.Input)).Value = new Akomi.InformationModel.Product.ProductBase() { PhysicalDescription = physDescr };
            //opcUaParams.CurrentDepth = 0;
            //opcUaParams.MaximumDepth = 3000;

            // It's more comfortable to use only OpcUaStartupParameters for server configuration and try to add missing options in it on demand
            //opcUaParams.TryUseAppConfigFile = false; // If this is true "AkomiServer.dll.config" properties will be used for server specifications
            //opcUaParams.UserCanSearchForConfigurationFile = false;
            opcUaParams.Ports = new[] { 48060 };
            opcUaParams2.Ports = new[] { 48061 };
            #endregion
            //opcUaParams.InheritedObjectSimplification = new List<Type>() { typeof(IDevice) };
            server.StartServer(robot, opcUaParams);
            server2.StartServer(retainer, opcUaParams2);

            Console.WriteLine("press any Key to exit ...");
            Console.ReadKey();
        }
    }
}
