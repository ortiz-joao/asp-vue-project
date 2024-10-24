const { createApp, ref, computed, defineProps } = Vue

const resultDisplay = {
	props: ['result'],
	template: `
		<div class="result-container">
			<strong style="font-size: 22px; width: 100%;">{{ result.query }}</strong>
			<template v-if="result.result">
				<p style="font-size: 18px; font-weight: 600; border-bottom: solid 2px #d3d3d3;">Distance from London Heathrow airport</p>
					<div class="distances-container">
						<p>{{ result.result.airport_distance_km.toFixed(2) }}Km</p>
						<p>{{ result.result.airport_distance_mi.toFixed(2) }}Mi</p>
					</div>
				</template>
			<p v-else>
				<p>Not Found.</p> <p>Please check if the postcode is correct.</p>
			</p>
		</div>
	`
}

createApp({
	components: {resultDisplay},
	setup() {
		const postcodes = ref(new Set()), results = ref([])
		const error = ref(null), input = ref(""), loading = ref(false), filter =ref("ascending")
		const history = localStorage.getItem("history")

		if(history) {
			results.value = JSON.parse(history)
		}

		const fetch_postcodes = async () => {
			try {
			error.value = null;
			loading.value = true
			const response = await fetch('/api/postcode', {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
				body: JSON.stringify({
					postcodes: Array.from(postcodes.value)
				})
			})
			
			const data = await response.json()
			if(response.ok) {
				results.value = data
				localStorage.setItem("history", JSON.stringify(results.value.slice(0, 3)))
			} else {
				error.value = data.message
			}
			
		} catch (err) {
			error.value = 'An error occurred while fetching postcodes.'
			console.error(err)
		} finally {
			loading.value = false
		}
		}

		function handle_text_area() {
		if(input.value) {
			if(input.value.indexOf(',') === -1){
				postcodes.value.add(input.value)
				
			}else {
				input.value.split(',').forEach((value) => {
					postcodes.value.add(value)
				})
			}
			input.value = ''
		}
		}

		function handle_postcode_delete(postcode) {
			postcodes.value.delete(postcode)
		}


		const orderedResults = computed(() => {
			if(filter.value === 'ascending'){
				return results.value.sort((a, b) => {
					if(a.result && b.result) {
						return a.result.airport_distance_km - b.result.airport_distance_km
					} else {
						return 0
					}
				})
			}else if(filter.value === "descending") {
				return results.value.sort((a, b) => {
					if(a.result && b.result) {
						return b.result.airport_distance_km - a.result.airport_distance_km 
					} else {
						return 0
					}
				})
			} 
			return results.value
			
		})
		return {
			postcodes, input, orderedResults, error, handle_postcode_delete, fetch_postcodes, handle_text_area, filter
		}
	}
}).mount("#postcodeApp")