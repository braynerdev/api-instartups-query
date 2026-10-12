export default {
    theme: "moon",
    layout: "classic",
    hideDarkModeToggle: true,
    
    authentication: {
      preferredSecurityScheme: "Bearer"
    },
    persistAuth: false,
    
    defaultHttpClient: {
      targetKey: "Node.js",
      clientKey: "Fetch"
    },
    
    showDeveloperTools: "never",
    
    isEditable: false,
    hideClientButton: true,
    hideTestRequestButton: false,
    
    agent: {
      disabled: true
    },
    
    telemetry: false
}