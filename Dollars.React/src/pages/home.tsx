import { AppSidebar } from "@/components/app-sidebar"
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbPage,
} from "@/components/ui/breadcrumb"
import { Separator } from "@/components/ui/separator"
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/lib/auth";

const Home = () => {

  const { logout } = useAuth();

  const handleClick = () => {
    logout();
  };

  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-16 shrink-0 items-center gap-2 border-b px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb>
            <BreadcrumbList>
              <BreadcrumbItem>
                <BreadcrumbPage>Dashboard</BreadcrumbPage>
              </BreadcrumbItem>
            </BreadcrumbList>
          </Breadcrumb>
          <div className="ml-auto">
            <Button variant="outline" size="sm" onClick={logout}>
              Sign out
            </Button>
          </div>
        </header>
        <div className="flex flex-1 flex-col gap-4 p-6">
          <h1 className="text-2xl font-semibold">Dashboard</h1>
          <p className="text-muted-foreground">the dollars</p>
        </div>
      </SidebarInset>
    </SidebarProvider>
  )
}

export default Home