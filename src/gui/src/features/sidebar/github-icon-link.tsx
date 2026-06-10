import githubBlack from "@/assets/icons/github_invertocat_black.svg";
import githubWhite from "@/assets/icons/github_invertocat_white.svg";

export function GitHubIconLink() {
  return (
    <a
      href="https://github.com/grantcolley/cryoptix"
      target="_blank"
      rel="noopener noreferrer"
      className="text-neutral-600 dark:text-neutral-300 hover:text-black dark:hover:text-white transition"
    >
      <img src={githubBlack} className="h-5 w-5 dark:hidden" />
      <img src={githubWhite} className="hidden h-5 w-5 dark:block" />
    </a>
  );
}
