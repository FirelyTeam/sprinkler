<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="03125e62-5d4c-47cf-bd08-f321a212105b" namespace="Furore.Fhir.Sprinkler.Framework.Configuration" xmlSchemaNamespace="urn:Furore.Fhir.Sprinkler.Framework.Configuration" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
  </typeDefinitions>
  <configurationElements>
    <configurationSection name="TestAssembliesConfiguration" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="testAssembliesConfiguration">
      <elementProperties>
        <elementProperty name="TestAssemblies" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="testAssemblies" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/03125e62-5d4c-47cf-bd08-f321a212105b/TestAssemblies" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElementCollection name="TestAssemblies" xmlItemName="testAssembly" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/03125e62-5d4c-47cf-bd08-f321a212105b/TestAssembly" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="TestAssembly">
      <attributeProperties>
        <attributeProperty name="AssemblyName" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="assemblyName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/03125e62-5d4c-47cf-bd08-f321a212105b/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>