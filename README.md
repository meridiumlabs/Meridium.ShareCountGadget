# Meridium.ShareCountGadget
An EPiServer dashboard gadget with an overview of the your total share count

## Usage

1. Install via NuGet
2. Configure: ```
  <episerver.shell>
    <publicModules rootPath="~/modules/" autoDiscovery="Modules">
      <add name="Meridium.ShareCountGadget">
        <assemblies>
          <add assembly="Meridium.ShareCountGadget" />
        </assemblies>
      </add>
    </publicModules>
  </episerver.shell>```
3. Implement `IShareCount` on all content types that you wish to save data for. _Pro tip_, use `[ScaffoldColumn(false)]` to hide the local block property.
4. Run the scheduled service to update the share count
5. Add the gadget to the dashboard
6. Use the custom report
