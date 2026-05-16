import { CryoptixLogo } from "@/components/icon/CryoptixLogo";

export function HomePage() {
  return (
    <main className="flex flex-1 items-center justify-center bg-background">
      <div className="flex h-96 w-96 items-center justify-center">
        <CryoptixLogo variant="logo" className="h-full w-full object-contain" />
      </div>
    </main>
  );
}
