$root = "C:\Users\yorze\OneDrive\Documentos\GitHub\SEGEDE_Grupo1\WebApp\Pages"

function New-RazorPage($folder, $pageName, $title, $namespaceSuffix) {
    $dir = Join-Path $root $folder
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    
    $cshtmlPath = Join-Path $dir "$pageName.cshtml"
    $csPath = Join-Path $dir "$pageName.cshtml.cs"
    
    $ns = "SEGEDE_Grupo1.WebApp.Pages"
    if ($namespaceSuffix -ne "") {
        $ns = "SEGEDE_Grupo1.WebApp.Pages.$namespaceSuffix"
    }
    
    $cshtmlContent = @"
@page
@model ${ns}.${pageName}Model
@{
    ViewData["Title"] = "$title";
}

<div class="container mt-4">
    <h2>@ViewData["Title"]</h2>
    <p class="text-muted">Esqueleto de la vista $title según documento técnico §27/§28.</p>
</div>
"@

    $csContent = @"
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ${ns};

// TODO: PageModel para $title consumiendo la Web API REST.
public class ${pageName}Model : PageModel
{
    public void OnGet() { }
}
"@

    Set-Content -Path $cshtmlPath -Value $cshtmlContent.Trim() -Encoding UTF8 -Force
    Set-Content -Path $csPath -Value $csContent.Trim() -Encoding UTF8 -Force
    Write-Host "Creada página: $folder\$pageName"
}

# 1. Páginas Públicas (8)
New-RazorPage "" "Index" "Home" ""
New-RazorPage "" "Login" "Login" ""
New-RazorPage "" "LoginOtp" "Login OTP Verification" ""
New-RazorPage "" "Register" "User Registration" ""
New-RazorPage "" "Activate" "Account Activation" ""
New-RazorPage "" "RecoverPassword" "Recover Password" ""
New-RazorPage "" "ResetPassword" "Reset Password" ""
New-RazorPage "" "AccessDenied" "Access Denied" ""

# 2. Administrator (15)
New-RazorPage "Admin" "Dashboard" "Admin Dashboard" "Admin"
New-RazorPage "Admin" "Users" "User Management" "Admin"
New-RazorPage "Admin" "Turbines" "Turbine Management" "Admin"
New-RazorPage "Admin" "TurbineDetail" "Turbine Detail" "Admin"
New-RazorPage "Admin" "Flush" "Flush Control & Execution" "Admin"
New-RazorPage "Admin" "CentralBank" "Central Bank Inventory" "Admin"
New-RazorPage "Admin" "Forecasts" "Energy Forecasts" "Admin"
New-RazorPage "Admin" "Distribution" "Commercial Distribution" "Admin"
New-RazorPage "Admin" "Prices" "Energy Prices" "Admin"
New-RazorPage "Admin" "Taxes" "Tax Configuration" "Admin"
New-RazorPage "Admin" "Statements" "Account Statements" "Admin"
New-RazorPage "Admin" "Audit" "Audit Logs" "Admin"
New-RazorPage "Admin" "Exports" "Export Logs & Files" "Admin"
New-RazorPage "Admin" "Maintenances" "Maintenance Overview" "Admin"
New-RazorPage "Admin" "Failures" "Failure Reports" "Admin"

# 3. Engineer (9)
New-RazorPage "Engineer" "Dashboard" "Operations Dashboard" "Engineer"
New-RazorPage "Engineer" "Turbines" "Turbines Operations" "Engineer"
New-RazorPage "Engineer" "TurbineDetail" "Turbine Technical Detail" "Engineer"
New-RazorPage "Engineer" "Maintenances" "Maintenance Scheduling & Execution" "Engineer"
New-RazorPage "Engineer" "Failures" "Failure Logging & Analysis" "Engineer"
New-RazorPage "Engineer" "Energy" "Energy Generation & Losses" "Engineer"
New-RazorPage "Engineer" "CentralBank" "Central Bank Monitoring" "Engineer"
New-RazorPage "Engineer" "FlushHistory" "Flush Operational History" "Engineer"
New-RazorPage "Engineer" "Audit" "Technical Audit Logs" "Engineer"

# 4. Buyer (5)
New-RazorPage "Buyer" "Dashboard" "Buyer Dashboard" "Buyer"
New-RazorPage "Buyer" "Forecasts" "My Demand Forecasts" "Buyer"
New-RazorPage "Buyer" "Statements" "My Account Statements" "Buyer"
New-RazorPage "Buyer" "Distributions" "My Energy Distributions" "Buyer"
New-RazorPage "Buyer" "Profile" "My Profile" "Buyer"

Write-Host "Todas las 37 páginas del inventario (§27) generadas exitosamente."
