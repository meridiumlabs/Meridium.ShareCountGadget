# Meridium.ShareCountGadget
An EPiServer dashboard gadget with an overview of the your total share count

## Usage

1. Implement `IShareCount` on all content types that you wish to save data for. _Pro tip_, use `[ScaffoldColumn(false)]` to hide the local block property.
2. Run the scheduled service to update the share count
3. Add the gadget to the dashboard 
