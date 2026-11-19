<!--
  The game's one back button, picked by Jake on 2026-11-19 as take A in
  static/mock/hub-and-back.html: the Settings and Info button from the native
  home (translucent black, 12px radius, 42px tall), the native arrow, and the
  press that scales to 0.9. Under 640px it keeps the arrow and hides the label.
-->
<script lang="ts">
	import { base } from '$app/paths';

	let { label, href, onclick }: { label: string; href?: string; onclick?: () => void } = $props();
</script>

{#if href}
	<a class="back" {href}>
		<img src="{base}/images/classic/arrow.png" alt="" />
		<span>{label}</span>
	</a>
{:else}
	<button class="back" type="button" {onclick}>
		<img src="{base}/images/classic/arrow.png" alt="" />
		<span>{label}</span>
	</button>
{/if}

<style>
	.back {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		flex: none;
		height: 42px;
		padding: 0 16px 0 12px;
		border: 0;
		border-radius: 12px;
		background: rgba(0, 0, 0, 0.8);
		color: #fff;
		font: 600 14px/1 var(--font-body);
		text-decoration: none;
		cursor: pointer;
		transition: transform 120ms ease, opacity 120ms ease;
	}
	.back img { display: block; height: 16px; width: auto; }
	.back:hover { opacity: 0.9; background: rgba(0, 0, 0, 0.62); }
	.back:active { transform: scale(0.9); opacity: 0.9; }
	.back:focus-visible { outline: 2px solid var(--color-accent-primary); outline-offset: 2px; }

	@media (max-width: 640px) {
		.back { padding: 0 12px; }
		.back span {
			position: absolute;
			width: 1px;
			height: 1px;
			overflow: hidden;
			clip-path: inset(50%);
		}
	}
</style>
